using ApiUp.Context;
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
        public ModelsController(ModelsContext context) { _context = context; }

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
            try
            {
                var item = await _context.Models.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Модель с ID {id} не найдена");

                _context.Models.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Модель удалена" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
