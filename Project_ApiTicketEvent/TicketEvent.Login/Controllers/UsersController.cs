using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Requests;
using Repositories.Interfaces;

namespace TicketEvent.Login.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly INguoiDungRepository _userRepo;

        public UsersController(INguoiDungRepository userRepo)
        {
            _userRepo = userRepo;
        }


        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userRepo.GetAll();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var user = _userRepo.GetById(id);
            if (user == null) return NotFound();

            return Ok(user);
        }
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] UpdateUserRequest request)
        {
            var user = _userRepo.GetById(id);
            if (user == null) return NotFound();

            user.HoTen = request.HoTen;
            user.VaiTroId = request.VaiTroId;
            user.TrangThai = request.TrangThai;
            user.Email = request.Email;
            user.SoDienThoai = request.SoDienThoai;
            var ok = _userRepo.Update(user);
            if (!ok) return StatusCode(500, "Không thể cập nhật người dùng.");

            return NoContent();
        }
        [HttpDelete("{id:int}")]
        public IActionResult SoftDelete(int id)
        {
            var ok = _userRepo.SoftDelete(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
