using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/DevelopersController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class DevelopersController : Controller
    {
        private readonly DevelopersContext _context;
        public DevelopersController(DevelopersContext context) { _context = context; }

        [Route("List")]
        [HttpGet]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.Developers.ToListAsync()); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.Developers.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Разработчик с ID {id} не найден");
                return Ok(item);
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] Developer item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.Developers.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Разработчик создан", id = item.id });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] Developer dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var item = await _context.Developers.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Разработчик с ID {id} не найден");

                item.name = dto.name;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Разработчик обновлен" });
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
                var item = await _context.Developers.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Разработчик с ID {id} не найден");

                _context.Developers.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Разработчик удалзработчик удален" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
