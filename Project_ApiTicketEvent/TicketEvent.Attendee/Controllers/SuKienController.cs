using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.Implementations;
using Repositories.Interfaces;

namespace TicketEvent.Attendee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuKienController : ControllerBase
    {
        private readonly ISuKienRepository _repo;

        public SuKienController(ISuKienRepository repo)
        {
            _repo = repo;
        }
        // GET: api/sukien
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
        // GET: /api/SuKien/by-name?ten=Concert
        [HttpGet("by-name")]
        public async Task<IActionResult> GetByName([FromQuery] string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
                return BadRequest(new { message = "Thiếu query parameter: ten" });

            var data = await _repo.GetByNameAsync(ten, trangThai: true);
            return Ok(data);
        }

        // GET: /api/SuKien/by-category?id=3
        // hoặc GET: /api/SuKien/by-category?tenDanhMuc=Workshop
        [HttpGet("by-category")]
        public async Task<IActionResult> GetByCategory( [FromQuery] string? tenDanhMuc)
        {
            if ( string.IsNullOrWhiteSpace(tenDanhMuc))
                return BadRequest(new { message = "Cần truyền tenDanhMuc" });

            var dataByName = await _repo.GetByDanhMucNameAsync(tenDanhMuc!, trangThai: true);
            return Ok(dataByName);
        }
    }
}
