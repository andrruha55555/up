using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/InventoryItemsController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class InventoryItemsController : Controller
    {
        private readonly InventoryItemsContext _context;
        public InventoryItemsController(InventoryItemsContext context) { _context = context; }

        [Route("List")]
        [HttpGet]
        [ProducesResponseType(typeof(List<InventoryItem>), 200)]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.InventoryItems.ToListAsync()); }
            catch (Exception exp) { Console.WriteLine($"Error in InventoryItems List: {exp.Message}"); return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        [ProducesResponseType(typeof(InventoryItem), 200)]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.InventoryItems.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Позиция инвентаризации с ID {id} не найдена");
                return Ok(item);
            }
            catch (Exception exp) { Console.WriteLine($"Error in InventoryItems Item: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] InventoryItem item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.InventoryItems.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Позиция инвентаризации создана", id = item.id });
            }
            catch (Exception exp) { Console.WriteLine($"Error in InventoryItems Add: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] InventoryItem dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var item = await _context.InventoryItems.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Позиция инвентаризации с ID {id} не найдена");

                item.inventory_id = dto.inventory_id;
                item.equipment_id = dto.equipment_id;
                item.checked_by_user_id = dto.checked_by_user_id;
                item.comment = dto.comment;
                item.checked_at = dto.checked_at;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Позиция инвентаризации обновлена" });
            }
            catch (Exception exp) { Console.WriteLine($"Error in InventoryItems Update: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Delete")]
        [HttpDelete]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var item = await _context.InventoryItems.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Позиция инвентаризации с ID {id} не найдена");

                _context.InventoryItems.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Позиция инвентаризации удалена" });
            }
            catch (Exception exp) { Console.WriteLine($"Error in InventoryItems Delete: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("TestConnection")]
        [HttpGet]
        public async Task<ActionResult> TestConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                return Ok(new { canConnect, database = _context.Database.GetDbConnection().Database, server = _context.Database.GetDbConnection().DataSource });
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message, innerError = ex.InnerException?.Message }); }
        }
    }
}
