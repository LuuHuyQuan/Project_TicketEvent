using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Requests;
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

        [HttpGet("me")]
        public IActionResult GetMyTickets([FromQuery] int nguoiSoHuuId)
        {
            if (nguoiSoHuuId <= 0)
                return BadRequest(new { message = "nguoiSoHuuId invalid" });

            var data = _repo.GetMyTickets(nguoiSoHuuId);
            return Ok(data);
        }

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

        [HttpPatch("huy/{maVe}")]
        public async Task<IActionResult> HuyVe(
            [FromRoute] string maVe,
            [FromQuery] int nguoiSoHuuId,
            [FromBody] HuyVeRequest? body)
        {
            if (nguoiSoHuuId <= 0) return BadRequest(new { message = "nguoiSoHuuId invalid" });
            if (string.IsNullOrWhiteSpace(maVe)) return BadRequest(new { message = "maVe invalid" });

            var ok = await _repo.HuyVeAsync(nguoiSoHuuId, maVe, body?.LyDo);
            if (!ok)
                return BadRequest(new { message = "Không thể hủy (vé không tồn tại/không thuộc về bạn/đã dùng/đã hủy/đã hoàn)." });

            return Ok(new { message = "Đã hủy vé." });
        }

        [HttpPost("hoan/{maVe}")]
        public async Task<IActionResult> HoanVe(
            [FromRoute] string maVe,
            [FromQuery] int nguoiSoHuuId,
            [FromBody] HoanVeRequest body)
        {
            if (nguoiSoHuuId <= 0) return BadRequest(new { message = "nguoiSoHuuId invalid" });
            if (string.IsNullOrWhiteSpace(maVe)) return BadRequest(new { message = "maVe invalid" });

            var result = await _repo.HoanVeAsync(nguoiSoHuuId, maVe, body?.LyDo, body?.PhuongThuc, body?.RawResponse);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }
    }
}
