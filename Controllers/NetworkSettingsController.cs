using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/NetworkSettingsController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class NetworkSettingsController : Controller
    {
        private readonly NetworkSettingsContext _context;
        public NetworkSettingsController(NetworkSettingsContext context) { _context = context; }

        [Route("List")]
        [HttpGet]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.NetworkSettings.ToListAsync()); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.NetworkSettings.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Сетевые настройки с ID {id} не найдены");
                return Ok(item);
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] NetworkSetting item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.NetworkSettings.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Сетевые настройки созданы", id = item.id });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] NetworkSetting dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var item = await _context.NetworkSettings.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Сетевые настройки с ID {id} не найдены");

                item.equipment_id = dto.equipment_id;
                item.ip_address = dto.ip_address;
                item.subnet_mask = dto.subnet_mask;
                item.gateway = dto.gateway;
                item.dns1 = dto.dns1;
                item.dns2 = dto.dns2;
                item.created_at = dto.created_at;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Сетевые настройки обновлены" });
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
                var item = await _context.NetworkSettings.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Сетевые настройки с ID {id} не найдены");

                _context.NetworkSettings.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Сетевые настройки удалены" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
