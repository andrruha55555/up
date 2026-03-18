using ApiUp.Context;
using Microsoft.EntityFrameworkCore;
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
        private readonly EquipmentContext _equipmentContext;
        public StatusesController(StatusesContext context, EquipmentContext equipmentContext)
        { _context = context; _equipmentContext = equipmentContext; }

        [HttpGet("List")]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.Statuses.ToListAsync()); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [HttpGet("Item")]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.Statuses.FirstOrDefaultAsync(x => x.id == id);
                if (item == null) return NotFound($"Статус с ID {id} не найден");
                return Ok(item);
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [HttpPost("Add")]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] Status item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                item.id = 0;
                _context.Statuses.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Статус создан", id = item.id });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [HttpPut("Update")]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] Status dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var item = await _context.Statuses.FirstOrDefaultAsync(x => x.id == id);
                if (item == null) return NotFound($"Статус с ID {id} не найден");
                item.name = dto.name;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Статус обновлен" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [HttpDelete("Delete")]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            var hasEquipment = await _equipmentContext.Equipment.AnyAsync(e => e.status_id == id);
            if (hasEquipment) return StatusCode(409, "Невозможно удалить статус: он используется в оборудовании.");

            try
            {
                var item = await _context.Statuses.FirstOrDefaultAsync(x => x.id == id);
                if (item == null) return NotFound($"Статус с ID {id} не найден");
                _context.Statuses.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Статус удален" });
            }
            catch (DbUpdateException)
            {
                return StatusCode(409, "Невозможно удалить статус: он используется в оборудовании.");
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}