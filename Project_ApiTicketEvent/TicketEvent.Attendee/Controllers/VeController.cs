using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace TicketEvent.Attendee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeController : ControllerBase
    {
        private readonly IVeRepository _repo;

        public VeController(IVeRepository repo)
        {
            _repo = repo;
        }

        // GET /api/Ve/me?nguoiSoHuuId=3
        [HttpGet("me")]
        public IActionResult GetMyTickets([FromQuery] int nguoiSoHuuId)
        {
            if (nguoiSoHuuId <= 0)
                return BadRequest(new { message = "nguoiSoHuuId invalid" });

            var data = _repo.GetMyTickets(nguoiSoHuuId);
            return Ok(data);
        }

        // GET /api/Ve/{maVe}?nguoiSoHuuId=3
        [HttpGet("{maVe}")]
        public IActionResult GetMyTicketDetail([FromRoute] string maVe, [FromQuery] int nguoiSoHuuId)
        {
            if (nguoiSoHuuId <= 0)
                return BadRequest(new { message = "nguoiSoHuuId invalid" });

            var item = _repo.GetMyTicketByMaVe(nguoiSoHuuId, maVe);
            if (item == null)
                return NotFound(new { message = "Không tìm thấy vé hoặc vé không thuộc về bạn." });

            return Ok(item);
        }
    }
}
