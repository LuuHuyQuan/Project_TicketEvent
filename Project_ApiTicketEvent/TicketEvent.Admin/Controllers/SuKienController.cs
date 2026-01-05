using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.Interfaces;

namespace TicketEvent.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/sukien")]
    public class SuKienController : ControllerBase
    {
        private readonly ISuKienRepository _repo;

        public SuKienController(ISuKienRepository repo)
        {
            _repo = repo;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SuKien>>> GetAll()
        {
            try
            {
                var suKiens = await _repo.GetAllAsync();
                return Ok(suKiens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách sự kiện", error = ex.Message });
            }
        }
        [HttpGet("by-name")]
        public async Task<IActionResult> GetByName([FromQuery] string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
                return BadRequest(new { message = "Thiếu query parameter: ten" });

            var data = await _repo.GetByNameAsync(ten, trangThai: true);
            return Ok(data);
        }

      
        [HttpGet("by-category")]
        public async Task<IActionResult> GetByCategory([FromQuery] string? tenDanhMuc)
        {
            if (string.IsNullOrWhiteSpace(tenDanhMuc))
                return BadRequest(new { message = "Cần truyền tenDanhMuc" });

            var dataByName = await _repo.GetByDanhMucNameAsync(tenDanhMuc!, trangThai: true);
            return Ok(dataByName);
        }
        [HttpGet("pending")]
        public IActionResult GetPending()
        {
            var data = _repo.GetPending();
            return Ok(data);
        }

        [HttpPut("{id:int}/approve")]
        public IActionResult Approve(int id)
        {
            var ok = _repo.Approve(id);
            if (!ok)
                return Conflict(new
                {
                    message = "Duyệt thất bại. Sự kiện không tồn tại hoặc không còn ở trạng thái chờ duyệt (0)."
                });

            return Ok(new { message = "Duyệt sự kiện thành công (TrangThai = 1)." });
        }

        [HttpPut("{id:int}/cancel")]
        public IActionResult Cancel(int id)
        {
            var ok = _repo.Cancel(id);
            if (!ok)
                return Conflict(new
                {
                    message = "Huỷ thất bại. Sự kiện không tồn tại hoặc không còn ở trạng thái chờ duyệt (0)."
                });

            return Ok(new { message = "Huỷ sự kiện thành công (TrangThai = 5)." });
        }
    }
}
