using ApiUp.Context;
using ApiUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Controllers
{
    [Route("api/UsersController")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class UsersController : Controller
    {
        private readonly UsersContext _context;
        public UsersController(UsersContext context) { _context = context; }

        [Route("List")]
        [HttpGet]
        [ProducesResponseType(typeof(List<User>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> List()
        {
            try { return Ok(await _context.Users.ToListAsync()); }
            catch (Exception exp) { Console.WriteLine($"Error in Users List: {exp.Message}"); return StatusCode(500, exp.Message); }
        }

        [Route("Item")]
        [HttpGet]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> Item(int id)
        {
            try
            {
                var item = await _context.Users.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Пользователь с ID {id} не найден");
                return Ok(item);
            }
            catch (Exception exp) { Console.WriteLine($"Error in Users Item: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Add")]
        [HttpPost]
        [ApiExplorerSettings(GroupName = "v2")]
        public async Task<ActionResult> Add([FromBody] User item)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                _context.Users.Add(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Пользователь создан", id = item.id });
            }
            catch (Exception exp) { Console.WriteLine($"Error in Users Add: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Update")]
        [HttpPut]
        [ApiExplorerSettings(GroupName = "v3")]
        public async Task<ActionResult> Update(int id, [FromBody] User dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var item = await _context.Users.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Пользователь с ID {id} не найден");

                item.login = dto.login;
                item.password_hash = dto.password_hash;
                item.role = dto.role;
                item.email = dto.email;
                item.last_name = dto.last_name;
                item.first_name = dto.first_name;
                item.middle_name = dto.middle_name;
                item.phone = dto.phone;
                item.address = dto.address;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Пользователь обновлен" });
            }
            catch (Exception exp) { Console.WriteLine($"Error in Users Update: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }

        [Route("Delete")]
        [HttpDelete]
        [ApiExplorerSettings(GroupName = "v4")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var item = await _context.Users.Where(x => x.id == id).FirstOrDefaultAsync();
                if (item == null) return NotFound($"Пользователь с ID {id} не найден");

                _context.Users.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Пользователь удален" });
            }
            catch (Exception exp) { Console.WriteLine($"Error in Users Delete: {exp.Message}"); return StatusCode(500, "Internal server error"); }
        }
    }
}
