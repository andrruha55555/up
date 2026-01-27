using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/EquipmentController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class EquipmentController : Controller
    {
        private readonly EquipmentContext _context;
        public EquipmentController(EquipmentContext context) { _context = context; }

        [Route("List")]
        [HttpGet]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.Equipment.ToListAsync()); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.Equipment.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Оборудование с ID {id} не найдено");
                return Ok(item);
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] Equipment item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.Equipment.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Оборудование создано", id = item.id });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] Equipment dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var item = await _context.Equipment.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Оборудование с ID {id} не найдено");

                item.name = dto.name;
                item.inventory_number = dto.inventory_number;
                item.classroom_id = dto.classroom_id;
                item.responsible_user_id = dto.responsible_user_id;
                item.temp_responsible_user_id = dto.temp_responsible_user_id;
                item.cost = dto.cost;
                item.direction_id = dto.direction_id;
                item.status_id = dto.status_id;
                item.model_id = dto.model_id;
                item.comment = dto.comment;
                item.image_path = dto.image_path;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Оборудование обновлено" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Delete")]
        [HttpDelete]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var item = await _context.Equipment.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Оборудование с ID {id} не найдено");

                _context.Equipment.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Оборудование удалено" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
