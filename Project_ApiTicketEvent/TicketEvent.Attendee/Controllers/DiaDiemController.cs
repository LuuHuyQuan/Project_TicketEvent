using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace TicketEvent.Attendee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiaDiemController : ControllerBase
    {
        private readonly IDiaDiemService _service;

        public DiaDiemController(IDiaDiemService service)
        {
            _service = service;
        }

        // GET: /api/DiaDiem
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        // GET: /api/DiaDiem/by-name?ten=Nhà%20thi%20đấu
        [HttpGet("by-name")]
        public async Task<IActionResult> GetByName([FromQuery] string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
                return BadRequest(new { message = "Thiếu query parameter: ten" });

            var item = await _service.GetByNameAsync(ten);
            if (item == null)
                return NotFound(new { message = "Không tìm thấy địa điểm phù hợp." });

            return Ok(item);
        }
    }
}
