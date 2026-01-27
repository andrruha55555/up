using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/EquipmentSoftwareController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class EquipmentSoftwareController : Controller
    {
        private readonly EquipmentSoftwareContext _context;
        public EquipmentSoftwareController(EquipmentSoftwareContext context) { _context = context; }

        [Route("List")]
        [HttpGet]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.EquipmentSoftware.ToListAsync()); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        public async Task<ActionResult> Item(int equipmentId, int softwareId)
        {
            try
            {
                var item = await _context.EquipmentSoftware
                    .Where(x => x.equipment_id == equipmentId && x.software_id == softwareId)
                    .FirstOrDefaultAsync();

                if (item == null) return NotFound($"Связь equipment={equipmentId}, software={softwareId} не найдена");
                return Ok(item);
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] EquipmentSoftware item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.EquipmentSoftware.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Связь создана", equipment_id = item.equipment_id, software_id = item.software_id });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
