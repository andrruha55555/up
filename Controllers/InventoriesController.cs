using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/InventoriesController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class InventoriesController : Controller
    {
        private readonly InventoriesContext _context;
        public InventoriesController(InventoriesContext context) { _context = context; }

        [Route("List")]
        [HttpGet]
        [ProducesResponseType(typeof(List<Inventory>), 200)]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.Inventories.ToListAsync()); }
            catch (Exception exp) { Console.WriteLine($"Error in Inventories List: {exp.Message}"); return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        [ProducesResponseType(typeof(Inventory), 200)]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.Inventories.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Инвентаризация с ID {id} не найдена");
                return Ok(item);
            }
            catch (Exception exp) { Console.WriteLine($"Error in Inventories Item: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] Inventory item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.Inventories.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Инвентаризация создана", id = item.id });
            }
            catch (Exception exp) { Console.WriteLine($"Error in Inventories Add: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] Inventory dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var item = await _context.Inventories.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Инвентаризация с ID {id} не найдена");

                item.name = dto.name;
                item.start_date = dto.start_date;
                item.end_date = dto.end_date;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Инвентаризация обновлена" });
            }
            catch (Exception exp) { Console.WriteLine($"Error in Inventories Update: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Delete")]
        [HttpDelete]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var item = await _context.Inventories.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Инвентаризация с ID {id} не найдена");

                _context.Inventories.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Инвентаризация удалена" });
            }
            catch (Exception exp) { Console.WriteLine($"Error in Inventories Delete: {exp.Message}"); return StatusCode(500, "Internal server error"); }
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
