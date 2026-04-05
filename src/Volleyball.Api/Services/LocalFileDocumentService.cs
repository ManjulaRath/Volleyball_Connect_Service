using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Volleyball.Application.Interfaces;
using Volleyball.Domain.Entities;
using Volleyball.Infrastructure.Data;

namespace Volleyball.Api.Services
{
    // Simple local file storage implementation kept in the Api project so web-specific types are available
    public class LocalFileDocumentService : IDocumentService
    {
        private readonly string _basePath;
        private readonly ApplicationDbContext _db;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

        public LocalFileDocumentService(IWebHostEnvironment env, IConfiguration config, ApplicationDbContext db)
        {
            _db = db;
            var configured = config["LocalStorage:BasePath"];
            _basePath = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(env.ContentRootPath, "Storage", "PlayerDocuments")
                : Path.GetFullPath(configured);
            Directory.CreateDirectory(_basePath);
        }

        public async Task<PlayerDocument> UploadPlayerDocumentAsync(Stream content, string fileName, string contentType, int playerId, DocumentType documentType, long? length = null)
        {
            if (content == null || !content.CanRead) throw new ArgumentException("Content stream is not readable", nameof(content));

            var ext = Path.GetExtension(fileName) ?? string.Empty;
            var storedFileName = $"{Guid.NewGuid()}{ext}";
            var relativeFolder = Path.Combine(playerId.ToString());
            var folder = Path.Combine(_basePath, relativeFolder);
            Directory.CreateDirectory(folder);
            var physicalPath = Path.Combine(folder, storedFileName);

            await using (var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await content.CopyToAsync(stream);
            }

            // store relative path in DB so swapping storage later is easier
            var relativePath = Path.Combine(relativeFolder, storedFileName).Replace('\\', '/');

            var doc = new PlayerDocument
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                DocumentType = documentType,
                DocumentUrl = relativePath,
                UploadedAt = DateTime.UtcNow
            };

            _db.PlayerDocuments.Add(doc);
            await _db.SaveChangesAsync();

            return doc;
        }

        public async Task DeletePlayerDocumentAsync(PlayerDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var physical = GetPhysicalPath(document.DocumentUrl);
            try
            {
                if (File.Exists(physical)) File.Delete(physical);
            }
            catch { /* log if available */ }

            _db.PlayerDocuments.Remove(document);
            await _db.SaveChangesAsync();
        }

        public Task<PlayerDocument?> GetDocumentByIdAsync(Guid id)
        {
            return _db.PlayerDocuments.FindAsync(id).AsTask();
        }

        public Task<(Stream Stream, string ContentType, string FileName)> OpenReadAsync(PlayerDocument document)
        {
            var physical = GetPhysicalPath(document.DocumentUrl);
            if (!File.Exists(physical)) throw new FileNotFoundException("Document not found", physical);

            var stream = new FileStream(physical, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            var fileName = Path.GetFileName(physical);

            if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return Task.FromResult<(Stream, string, string)>((stream, contentType, fileName));
        }

        private string GetPhysicalPath(string relativePath)
        {
            // relativePath stored as "playerId/filename.ext"
            var safe = relativePath.Replace('/', Path.DirectorySeparatorChar);
            return Path.GetFullPath(Path.Combine(_basePath, safe));
        }
    }
}
