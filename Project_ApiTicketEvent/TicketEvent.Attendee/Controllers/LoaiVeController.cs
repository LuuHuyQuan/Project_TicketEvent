using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using static Models.DTOs.Requests.LoaiVeRequest;

namespace TicketEvent.Attendee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiVeController : ControllerBase
    {
        private readonly ILoaiVeRepository _repo;

        public LoaiVeController(ILoaiVeRepository repo)
        {
            _repo = repo;
        }

        // GET: /api/LoaiVe
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Attendee: mặc định chỉ lấy loại vé đang bán (TrangThai=1)
            var data = await _repo.GetAllAsync(trangThai: true);
            return Ok(data);
        }

        // GET: /api/LoaiVe/by-name?ten=VIP
        [HttpGet("by-name")]
        public async Task<IActionResult> GetByName([FromQuery] GetLoaiVeByNameRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Ten))
                return BadRequest(new { message = "Thiếu query parameter: ten" });

            var data = await _repo.GetByNameAsync(req.Ten, trangThai: true);
            return Ok(data);
        }

        // GET: /api/LoaiVe/by-event-name?tenSuKien=Concert
        [HttpGet("by-event-name")]
        public async Task<IActionResult> GetByTenSuKien([FromQuery] GetLoaiVeByEventRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.TenSuKien))
                return BadRequest(new { message = "Thiếu query parameter: tenSuKien" });

            var data = await _repo.GetByTenSuKienAsync(req.TenSuKien, trangThai: true);
            return Ok(data);
        }
    }
}
