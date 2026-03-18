using ApiUp.Context;
using Microsoft.EntityFrameworkCore;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/ClassroomsController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class ClassroomsController : Controller
    {
        private readonly ClassroomsContext _context;
        private readonly EquipmentContext _equipmentContext;
        public ClassroomsController(ClassroomsContext context, EquipmentContext equipmentContext)
        { _context = context; _equipmentContext = equipmentContext; }

        [Route("List")]
        [HttpGet]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.Classrooms.ToListAsync()); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.Classrooms.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Аудитория с ID {id} не найдена");
                return Ok(item);
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] Classroom item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.Classrooms.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Аудитория создана", id = item.id });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] Classroom dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var item = await _context.Classrooms.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Аудитория с ID {id} не найдена");

                item.name = dto.name;
                item.short_name = dto.short_name;
                item.responsible_user_id = dto.responsible_user_id;
                item.temp_responsible_user_id = dto.temp_responsible_user_id;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Аудитория обновлена" });
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
                var hasEquipment = await _equipmentContext.Equipment.AnyAsync(e => e.classroom_id == id);
                if (hasEquipment)
                    return StatusCode(409, "Невозможно удалить: в этом кабинете есть оборудование.");
                var item = await _context.Classrooms.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Аудитория с ID {id} не найдена");

                _context.Classrooms.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Аудитория удалена" });
            }
            catch (DbUpdateException) { return StatusCode(409, "\u0421\u0432\u044f\u0437\u0430\u043d\u043d\u044b\u0435 \u0437\u0430\u043f\u0438\u0441\u0438 \u0435\u0449\u0451 \u0441\u0443\u0449\u0435\u0441\u0442\u0432\u0443\u044e\u0442. \u0423\u0434\u0430\u043b\u0438\u0442\u0435 \u0438\u0445 \u0441\u043d\u0430\u0447\u0430\u043b\u0430."); }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
