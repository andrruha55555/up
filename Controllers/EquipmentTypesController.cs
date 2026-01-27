using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/EquipmentTypesController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class EquipmentTypesController : Controller
    {
        private readonly EquipmentTypesContext _context;
        public EquipmentTypesController(EquipmentTypesContext context) { _context = context; }

        [HttpGet("List")]
        public async Task<ActionResult> List()
        {
            return Ok(await _context.EquipmentTypes.ToListAsync());
        }

        [HttpGet("Item")]
        public async Task<ActionResult> Item(int id)
        {
            var item = await _context.EquipmentTypes.FirstOrDefaultAsync(x => x.id == id);
            if (item == null) return NotFound($"Тип оборудования с ID {id} не найден");
            return Ok(item);
        }

        [HttpPost("Add")]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] EquipmentType item)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.EquipmentTypes.Add(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Тип оборудования создан", id = item.id });
        }

        [HttpPut("Update")]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] EquipmentType dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var item = await _context.EquipmentTypes.FirstOrDefaultAsync(x => x.id == id);
            if (item == null) return NotFound($"Тип оборудования с ID {id} не найден");

            item.name = dto.name;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Тип оборудования обновлен" });
        }

        [HttpDelete("Delete")]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            var item = await _context.EquipmentTypes.FirstOrDefaultAsync(x => x.id == id);
            if (item == null) return NotFound($"Тип оборудования с ID {id} не найден");

            _context.EquipmentTypes.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Тип оборудования удален" });
        }
    }
}
