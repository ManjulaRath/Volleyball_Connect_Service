using System;
using System.IO;
using System.Threading.Tasks;
using Volleyball.Domain.Entities;

namespace Volleyball.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<PlayerDocument> UploadPlayerDocumentAsync(Stream content, string fileName, string contentType, int playerId, DocumentType documentType, long? length = null);
        Task DeletePlayerDocumentAsync(PlayerDocument document);
        Task<PlayerDocument?> GetDocumentByIdAsync(Guid id);
        Task<(Stream Stream, string ContentType, string FileName)> OpenReadAsync(PlayerDocument document);
    }
}
