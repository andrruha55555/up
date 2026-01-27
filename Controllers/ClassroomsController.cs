using ApiUp.Context;
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
        public ClassroomsController(ClassroomsContext context) { _context = context; }

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
                var item = await _context.Classrooms.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Аудитория с ID {id} не найдена");

                _context.Classrooms.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Аудитория удалена" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }

        [Route("TestConnection")]
        [HttpGet]
        public async Task<ActionResult> TestConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                return Ok(new { canConnect, database = _context.Database.GetDbConnection().Database, server = _context.Database.GetDbConnection().DataSource });
            }
            catch (Exception ex) { return StatusCode(500, new { error = ex.Message, innerError = ex.InnerException?.Message }); }
        }
    }
}
