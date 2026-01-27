using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/StatusesController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class StatusesController : Controller
    {
        private readonly StatusesContext _context;
        public StatusesController(StatusesContext context) { _context = context; }

        [HttpGet("List")]
        public async Task<ActionResult> List()
        {
            return Ok(await _context.Statuses.ToListAsync());
        }

        [HttpGet("Item")]
        public async Task<ActionResult> Item(int id)
        {
            var item = await _context.Statuses.FirstOrDefaultAsync(x => x.id == id);
            if (item == null) return NotFound($"Статус с ID {id} не найден");
            return Ok(item);
        }

        [HttpPost("Add")]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] Status item)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Statuses.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Статус создан", id = item.id });
        }

        [HttpPut("Update")]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] Status dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var item = await _context.Statuses.FirstOrDefaultAsync(x => x.id == id);
            if (item == null) return NotFound($"Статус с ID {id} не найден");

            item.name = dto.name;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Статус обновлен" });
        }

        [HttpDelete("Delete")]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            var item = await _context.Statuses.FirstOrDefaultAsync(x => x.id == id);
            if (item == null) return NotFound($"Статус с ID {id} не найден");

            _context.Statuses.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Статус удален" });
        }
    }
}
