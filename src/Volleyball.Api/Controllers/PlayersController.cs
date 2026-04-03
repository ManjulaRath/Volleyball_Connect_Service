using Microsoft.AspNetCore.Mvc;
using Volleyball.Application.Services;
using Volleyball.Domain.Entities;
using System.Linq;

namespace Volleyball.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly PlayerService _service;
        public PlayersController(PlayerService service) { _service = service; }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> Register([FromBody] Player p)
        {
            await _service.RegisterAsync(p);
            return CreatedAtAction(nameof(GetById), new { id = p.Id }, p);
        }

        [HttpGet]
        public IActionResult Search([FromQuery] string? q, [FromQuery] string? position, [FromQuery] string? gender, [FromQuery] int? minAge, [FromQuery] int? maxAge, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _service.Search(q, position, gender, minAge, maxAge);
            var total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new { total, page, pageSize, items });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var p = _service.Search(null, null, null, null, null).FirstOrDefault(x => x.Id == id);
            if (p == null) return NotFound();
            return Ok(p);
        }
    }
}
