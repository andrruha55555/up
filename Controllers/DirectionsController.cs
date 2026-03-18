using ApiUp.Context;
using Microsoft.EntityFrameworkCore;
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
        private readonly EquipmentContext _equipmentContext;
        public DirectionsController(DirectionsContext context, EquipmentContext equipmentContext)
        { _context = context; _equipmentContext = equipmentContext; }

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
            if (item == null) return NotFound($"Направление ID {id} не найдено");
            return Ok(item);
        }

        [HttpPost("Add")]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] Direction item)
        {
            try
            {
                _context.Directions.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Добавлено", id = item.id });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [HttpPut("Update")]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] Direction dto)
        {
            try
            {
                var item = await _context.Directions.FirstOrDefaultAsync(x => x.id == id);
                if (item == null) return NotFound($"Направление ID {id} не найдено");
                item.name = dto.name;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Обновлено" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [HttpDelete("Delete")]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            var hasEquip = await _equipmentContext.Equipment.AnyAsync(e => e.direction_id == id);
            if (hasEquip) return StatusCode(409, "Невозможно удалить: есть оборудование с этим направлением.");

            try
            {
                var item = await _context.Directions.FirstOrDefaultAsync(x => x.id == id);
                if (item == null) return NotFound($"Направление ID {id} не найдено");
                _context.Directions.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Удалено" });
            }
            catch (DbUpdateException)
            {
                return StatusCode(409, "Невозможно удалить: есть связанное оборудование.");
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
