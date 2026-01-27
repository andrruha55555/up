using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/SoftwareController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class SoftwareController : Controller
    {
        private readonly SoftwareContext _context;
        public SoftwareController(SoftwareContext context) { _context = context; }

        [Route("List")]
        [HttpGet]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.Software.ToListAsync()); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.Software.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"ПО с ID {id} не найдено");
                return Ok(item);
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] SoftwareEntity item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.Software.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "ПО создано", id = item.id });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] SoftwareEntity dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var item = await _context.Software.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"ПО с ID {id} не найдено");

                item.name = dto.name;
                item.developer_id = dto.developer_id;
                item.version = dto.version;

                await _context.SaveChangesAsync();
                return Ok(new { message = "ПО обновлено" });
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
                var item = await _context.Software.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"ПО с ID {id} не найдено");

                _context.Software.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "ПО удалено" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
