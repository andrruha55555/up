using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/ConsumableCharacteristicsController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class ConsumableCharacteristicsController : Controller
    {
        private readonly ConsumableCharacteristicsContext _context;

        public ConsumableCharacteristicsController(ConsumableCharacteristicsContext context)
        {
            _context = context;
        }
        [HttpGet("List")]
        [ProducesResponseType(typeof(List<ConsumableCharacteristic>), 200)]
        public async Task<ActionResult> List()
        {
            try
            {
                return Ok(await _context.ConsumableCharacteristics.ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("Item")]
        [ProducesResponseType(typeof(ConsumableCharacteristic), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.ConsumableCharacteristics
                    .FirstOrDefaultAsync(x => x.id == id);

                if (item == null)
                    return NotFound($"Характеристика с ID {id} не найдена");

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("ListByConsumable")]
        [ProducesResponseType(typeof(List<ConsumableCharacteristic>), 200)]
        public async Task<ActionResult> ListByConsumable(int consumableId)
        {
            try
            {
                var items = await _context.ConsumableCharacteristics
                    .Where(x => x.consumable_id == consumableId)
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("Add")]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] ConsumableCharacteristic item)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _context.ConsumableCharacteristics.Add(item);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Характеристика добавлена",
                    id = item.id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut("Update")]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] ConsumableCharacteristic dto)
        {
            try
            {
                var item = await _context.ConsumableCharacteristics
                    .FirstOrDefaultAsync(x => x.id == id);

                if (item == null)
                    return NotFound($"Характеристика с ID {id} не найдена");

                item.characteristic_name = dto.characteristic_name;
                item.characteristic_value = dto.characteristic_value;
                item.consumable_id = dto.consumable_id;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Характеристика обновлена" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpDelete("Delete")]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var item = await _context.ConsumableCharacteristics
                    .FirstOrDefaultAsync(x => x.id == id);

                if (item == null)
                    return NotFound($"Характеристика с ID {id} не найдена");

                _context.ConsumableCharacteristics.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Характеристика удалена" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
