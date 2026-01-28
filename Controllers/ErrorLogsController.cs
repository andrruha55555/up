using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/ErrorLogsController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class ErrorLogsController : Controller
    {
        private readonly LogsContext _context;
        public ErrorLogsController(LogsContext context) { _context = context; }
        [Route("List")]
        [HttpGet]
        public async Task<ActionResult> List(int page = 1, int pageSize = 50)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 50;
                if (pageSize > 200) pageSize = 200;

                var query = _context.ErrorLogs.OrderByDescending(x => x.created_at);

                var total = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                return Ok(new { total, page, pageSize, items });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
        [Route("Item")]
        [HttpGet]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.ErrorLogs.FirstOrDefaultAsync(x => x.id == id);
                if (item == null) return NotFound($"Лог с ID {id} не найден");
                return Ok(item);
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
                var item = await _context.ErrorLogs.FirstOrDefaultAsync(x => x.id == id);
                if (item == null) return NotFound($"Лог с ID {id} не найден");

                _context.ErrorLogs.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Лог удален" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
        [Route("DeleteOlderThan")]
        [HttpDelete]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> DeleteOlderThan(int days = 30)
        {
            try
            {
                if (days < 1) days = 1;
                var border = DateTime.UtcNow.AddDays(-days);

                var items = await _context.ErrorLogs.Where(x => x.created_at < border).ToListAsync();
                _context.ErrorLogs.RemoveRange(items);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Удалено {items.Count} логов старше {days} дней" });
            }
            catch (Exception exp) { return StatusCode(500, exp.Message); }
        }
    }
}
