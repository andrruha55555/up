using ApiUp.Context;
using Microsoft.EntityFrameworkCore;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/ModelsController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class ModelsController : Controller
    {
        private readonly ModelsContext _context;
        private readonly EquipmentContext _equipmentContext;
        public ModelsController(ModelsContext context, EquipmentContext equipmentContext)
        { _context = context; _equipmentContext = equipmentContext; }

        [Route("List")]
        [HttpGet]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.Models.ToListAsync()); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.Models.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Модель с ID {id} не найдена");
                return Ok(item);
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] ModelEntity item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.Models.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Модель создана", id = item.id });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] ModelEntity dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var item = await _context.Models.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Модель с ID {id} не найдена");

                item.name = dto.name;
                item.equipment_type_id = dto.equipment_type_id;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Модель обновлена" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Delete")]
        [HttpDelete]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            var hasEquipment = await _equipmentContext.Equipment.AnyAsync(e => e.model_id == id);
            if (hasEquipment) return StatusCode(409, "Невозможно удалить: есть оборудование этой модели.");

            try
            {
                var item = await _context.Models.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Модель с ID {id} не найдена");

                _context.Models.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Модель удалена" });
            }
            catch (DbUpdateException) { return StatusCode(409, "\u0421\u0432\u044f\u0437\u0430\u043d\u043d\u044b\u0435 \u0437\u0430\u043f\u0438\u0441\u0438 \u0435\u0449\u0451 \u0441\u0443\u0449\u0435\u0441\u0442\u0432\u0443\u044e\u0442. \u0423\u0434\u0430\u043b\u0438\u0442\u0435 \u0438\u0445 \u0441\u043d\u0430\u0447\u0430\u043b\u0430."); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
