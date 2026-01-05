using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Requests;
using Repositories.Interfaces;

namespace TicketEvent.Login.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
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
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

            return Ok(user);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] UpdateUserRequest request)
        {
            var user = _userRepo.GetById(id);
            if (user == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.HoTen)) user.HoTen = request.HoTen;
            if (!string.IsNullOrWhiteSpace(request.Email)) user.Email = request.Email;
            if (!string.IsNullOrWhiteSpace(request.SoDienThoai)) user.SoDienThoai = request.SoDienThoai;
            if (!string.IsNullOrWhiteSpace(request.TenDangNhap)) user.TenDangNhap = request.TenDangNhap; 
            if (request.VaiTroId.HasValue) user.VaiTroId = request.VaiTroId;
            if (request.TrangThai.HasValue) user.TrangThai = request.TrangThai;
            var ok = _userRepo.Update(user);
            if (!ok) return StatusCode(500, new { message = "Không thể cập nhật người dùng." });

            var updated = _userRepo.GetById(id);
            return Ok(new
            {
                message = "Cập nhật thành công.",
                data = updated
            });
        }

        [HttpDelete("{id:int}")]
        public IActionResult SoftDelete(int id)
        {
            var ok = _userRepo.SoftDelete(id);
            if (!ok) return NotFound(new { message = "Không tìm thấy người dùng." });

            return Ok(new
            {
                message = "Xóa mềm thành công.",
                id = id
            });
        }
    }
}
