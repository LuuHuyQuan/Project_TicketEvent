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

        // A) GET /api/LoaiVe
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Attendee: mặc định chỉ lấy TrangThai=1
            var data = await _repo.GetAllAsync(trangThai: 1);
            return Ok(data);
        }

        // B) GET /api/LoaiVe/by-name?ten=VIP
        [HttpGet("by-name")]
        public async Task<IActionResult> GetByName([FromQuery] GetLoaiVeByNameRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Ten))
                return BadRequest(new { message = "Thiếu query parameter: ten" });

            var data = await _repo.GetByNameAsync(req.Ten, trangThai: 1);
            return Ok(data);
        }

        // C) GET /api/LoaiVe/by-event?suKienId=2
        [HttpGet("by-event")]
        public async Task<IActionResult> GetByEvent([FromQuery] GetLoaiVeByEventRequest req)
        {
            if (req == null || req.SuKienId <= 0)
                return BadRequest(new { message = "suKienId invalid" });

            var data = await _repo.GetBySuKienIdAsync(req.SuKienId, trangThai: 1);
            return Ok(data);
        }
    }
}
