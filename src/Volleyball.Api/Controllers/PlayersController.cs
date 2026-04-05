using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Volleyball.Application.Services;
using Volleyball.Application.Interfaces;
using Volleyball.Domain.Entities;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Volleyball.Api.Controllers
{
    [ApiController]

    public class PlayersController : ControllerBase
    {
        private readonly PlayerService _service;
        private readonly IDocumentService _documentService;

        public PlayersController(PlayerService service, IDocumentService documentService)
        {
            _service = service;
            _documentService = documentService;
        }

        [HttpPost]
        [Route("api/Players/Register")]
        public async System.Threading.Tasks.Task<IActionResult> Register([FromBody] Player p)
        {
            await _service.RegisterAsync(p);
            return CreatedAtAction(nameof(GetById), new { id = p.Id }, p);
        }

        [HttpGet]
        [Route("api/Players/Search")]
        public IActionResult Search([FromQuery] string? q, [FromQuery] string? position, [FromQuery] string? gender, [FromQuery] int? minAge, [FromQuery] int? maxAge, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _service.Search(q, position, gender, minAge, maxAge);
            var total = query.Count();
            var players = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new { total, page, pageSize, players });
        }

        [HttpGet]
        [Route("api/Players/GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var p = _service.Search(null, null, null, null, null).FirstOrDefault(x => x.Id == id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        // Upload: multipart/form-data with "file" and "documentType" fields
        [HttpPost]
        [Route("api/Players/{playerId}/Documents")]
        public async Task<IActionResult> UploadDocument(int playerId, [FromForm] IFormFile file, [FromForm] DocumentType documentType)
        {
            if (file == null) return BadRequest("File is required.");

            await using var stream = file.OpenReadStream();
            var doc = await _documentService.UploadPlayerDocumentAsync(stream, file.FileName, file.ContentType ?? "application/octet-stream", playerId, documentType, file.Length);
            return CreatedAtAction(nameof(GetDocument), new { id = doc.Id }, doc);
        }

        // Download a document by id
        [HttpGet]
        [Route("api/Players/Documents/{id}")]
        public async Task<IActionResult> GetDocument(Guid id)
        {
            var doc = await _documentService.GetDocumentByIdAsync(id);
            if (doc == null) return NotFound();

            try
            {
                var (stream, contentType, fileName) = await _documentService.OpenReadAsync(doc);
                return File(stream, contentType, fileName);
            }
            catch (System.IO.FileNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
