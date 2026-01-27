using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/DirectionsController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class DirectionsController : Controller
    {
        private readonly DirectionsContext _context;
        public DirectionsController(DirectionsContext context) { _context = context; }

        [HttpGet("List")]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.Directions.ToListAsync()); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [HttpGet("Item")]
        public async Task<ActionResult> Item(int id)
        {
            var item = await _context.Directions.FirstOrDefaultAsync(x => x.id == id);
            if (item == null) return NotFound($"Направление с ID {id} не найдено");
            return Ok(item);
        }

        [HttpPost("Add")]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] Direction item)
        {
            _context.Directions.Add(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Направление создано", id = item.id });
        }

        [HttpPut("Update")]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] Direction dto)
        {
            var item = await _context.Directions.FirstOrDefaultAsync(x => x.id == id);
            if (item == null) return NotFound($"Направление с ID {id} не найдено");

            item.name = dto.name;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Направление обновлено" });
        }

        [HttpDelete("Delete")]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            var item = await _context.Directions.FirstOrDefaultAsync(x => x.id == id);
            if (item == null) return NotFound($"Направление с ID {id} не найдено");

            _context.Directions.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Направление удалено" });
        }
    }
}
