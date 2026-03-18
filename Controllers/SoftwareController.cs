using ApiUp.Context;
using Microsoft.EntityFrameworkCore;
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
        private readonly EquipmentSoftwareContext _eqSwContext;
        public SoftwareController(SoftwareContext context, EquipmentSoftwareContext eqSwContext)
        { _context = context; _eqSwContext = eqSwContext; }

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
            var inUse = await _eqSwContext.EquipmentSoftware.AnyAsync(e => e.software_id == id);
            if (inUse) return StatusCode(409, "Невозможно удалить ПО: оно установлено на оборудовании.");

            try
            {
                var item = await _context.Software.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"ПО с ID {id} не найдено");

                _context.Software.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "ПО удалено" });
            }
            catch (DbUpdateException) { return StatusCode(409, "\u0421\u0432\u044f\u0437\u0430\u043d\u043d\u044b\u0435 \u0437\u0430\u043f\u0438\u0441\u0438 \u0435\u0449\u0451 \u0441\u0443\u0449\u0435\u0441\u0442\u0432\u0443\u044e\u0442. \u0423\u0434\u0430\u043b\u0438\u0442\u0435 \u0438\u0445 \u0441\u043d\u0430\u0447\u0430\u043b\u0430."); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
