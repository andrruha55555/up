using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/ConsumablesController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class ConsumablesController : Controller
    {
        private readonly ConsumablesContext _context;
        public ConsumablesController(ConsumablesContext context) { _context = context; }

        [Route("List")]
        [HttpGet]
        [ProducesResponseType(typeof(List<Consumable>), 200)]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.Consumables.ToListAsync()); }
            catch (Exception exp) { Console.WriteLine($"Error in Consumables List: {exp.Message}"); return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        [ProducesResponseType(typeof(Consumable), 200)]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.Consumables.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Расходник с ID {id} не найден");
                return Ok(item);
            }
            catch (Exception exp) { Console.WriteLine($"Error in Consumables Item: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] Consumable item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.Consumables.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Расходник создан", id = item.id });
            }
            catch (Exception exp) { Console.WriteLine($"Error in Consumables Add: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] Consumable dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var item = await _context.Consumables.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Расходник с ID {id} не найден");

                item.name = dto.name;
                item.description = dto.description;
                item.arrival_date = dto.arrival_date;
                item.image_path = dto.image_path;
                item.quantity = dto.quantity;
                item.responsible_user_id = dto.responsible_user_id;
                item.temp_responsible_user_id = dto.temp_responsible_user_id;
                item.consumable_type_id = dto.consumable_type_id;
                item.attached_to_equipment_id = dto.attached_to_equipment_id;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Расходник обновлен" });
            }
            catch (Exception exp) { Console.WriteLine($"Error in Consumables Update: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Delete")]
        [HttpDelete]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var item = await _context.Consumables.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Расходник с ID {id} не найден");

                _context.Consumables.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Расходник удален" });
            }
            catch (Exception exp) { Console.WriteLine($"Error in Consumables Delete: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }
    }
}
