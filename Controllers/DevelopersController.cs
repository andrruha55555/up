using ApiUp.Context;
using Microsoft.EntityFrameworkCore;
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
        private readonly SoftwareContext _softwareContext;
        public DevelopersController(DevelopersContext context, SoftwareContext softwareContext)
        { _context = context; _softwareContext = softwareContext; }

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
            var hasSoftware = await _softwareContext.Software.AnyAsync(s => s.developer_id == id);
            if (hasSoftware) return StatusCode(409, "Невозможно удалить: есть программы этого разработчика.");

            try
            {
                var item = await _context.Developers.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Разработчик с ID {id} не найден");

                _context.Developers.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Разработчик удалзработчик удален" });
            }
            catch (DbUpdateException) { return StatusCode(409, "\u0421\u0432\u044f\u0437\u0430\u043d\u043d\u044b\u0435 \u0437\u0430\u043f\u0438\u0441\u0438 \u0435\u0449\u0451 \u0441\u0443\u0449\u0435\u0441\u0442\u0432\u0443\u044e\u0442. \u0423\u0434\u0430\u043b\u0438\u0442\u0435 \u0438\u0445 \u0441\u043d\u0430\u0447\u0430\u043b\u0430."); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
