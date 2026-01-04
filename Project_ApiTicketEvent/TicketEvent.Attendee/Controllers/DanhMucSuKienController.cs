using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
namespace TicketEvent.Attendee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhMucSuKienController : ControllerBase
    {
        private readonly IDanhMucSuKienService _service;

        public DanhMucSuKienController(IDanhMucSuKienService service)
        {
            _service = service;
        }

        // GET: /api/DanhMucSuKien
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        // GET: /api/DanhMucSuKien/by-name?ten=Workshop
        [HttpGet("by-name")]
        public async Task<IActionResult> GetByName([FromQuery] string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
                return BadRequest(new { message = "Thiếu query parameter: ten" });

            var item = await _service.GetByNameAsync(ten);
            if (item == null)
                return NotFound(new { message = "Không tìm thấy danh mục phù hợp." });

            return Ok(item);
        }
    }
}
