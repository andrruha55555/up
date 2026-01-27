using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/ConsumableTypesController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class ConsumableTypesController : Controller
    {
        private readonly ConsumableTypesContext _context;
        public ConsumableTypesController(ConsumableTypesContext context) { _context = context; }

        [Route("List")]
        [HttpGet]
        [ProducesResponseType(typeof(List<ConsumableType>), 200)]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.ConsumableTypes.ToListAsync()); }
            catch (Exception exp) { Console.WriteLine($"Error in ConsumableTypes List: {exp.Message}"); return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        [ProducesResponseType(typeof(ConsumableType), 200)]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.ConsumableTypes.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Тип расходника с ID {id} не найден");
                return Ok(item);
            }
            catch (Exception exp) { Console.WriteLine($"Error in ConsumableTypes Item: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] ConsumableType item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.ConsumableTypes.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Тип расходника создан", id = item.id });
            }
            catch (Exception exp) { Console.WriteLine($"Error in ConsumableTypes Add: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] ConsumableType dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var item = await _context.ConsumableTypes.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Тип расходника с ID {id} не найден");

                item.name = dto.name;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Тип расходника обновлен" });
            }
            catch (Exception exp) { Console.WriteLine($"Error in ConsumableTypes Update: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Delete")]
        [HttpDelete]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var item = await _context.ConsumableTypes.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Тип расходника с ID {id} не найден");

                _context.ConsumableTypes.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Тип расходника удален" });
            }
            catch (Exception exp) { Console.WriteLine($"Error in ConsumableTypes Delete: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }
    }
}
