using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/EquipmentHistoryController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class EquipmentHistoryController : Controller
    {
        private readonly EquipmentHistoryContext _context;
        public EquipmentHistoryController(EquipmentHistoryContext context) { _context = context; }

        [Route("List")]
        [HttpGet]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.EquipmentHistory.ToListAsync()); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.EquipmentHistory.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"История с ID {id} не найдена");
                return Ok(item);
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] EquipmentHistory item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.EquipmentHistory.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Запись истории создана", id = item.id });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] EquipmentHistory dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var item = await _context.EquipmentHistory.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"История с ID {id} не найдена");

                item.equipment_id = dto.equipment_id;
                item.classroom_id = dto.classroom_id;
                item.responsible_user_id = dto.responsible_user_id;
                item.comment = dto.comment;
                item.changed_at = dto.changed_at;
                item.changed_by_user_id = dto.changed_by_user_id;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Запись истории обновлена" });
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
                var item = await _context.EquipmentHistory.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"История с ID {id} не найдена");

                _context.EquipmentHistory.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Запись истории удалена" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
