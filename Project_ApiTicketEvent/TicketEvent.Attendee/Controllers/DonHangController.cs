using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Requests;
using Repositories.Interfaces;

namespace TicketEvent.Attendee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly IDonHangRepository _repo;

        public DonHangController(IDonHangRepository repo)
        {
            _repo = repo;
        }

        // GET: /api/DonHang/me?nguoiMuaId=1
        [HttpGet("me")]
        public async Task<IActionResult> GetMyOrders([FromQuery] int nguoiMuaId)
        {
            if (nguoiMuaId <= 0) return BadRequest(new { message = "nguoiMuaId invalid" });
            var data = await _repo.GetByNguoiMuaAsync(nguoiMuaId);
            return Ok(data);
        }

        // GET: /api/DonHang/{donHangId}?nguoiMuaId=1
        [HttpGet("{donHangId:int}")]
        public async Task<IActionResult> GetDetail([FromRoute] int donHangId, [FromQuery] int nguoiMuaId)
        {
            if (nguoiMuaId <= 0) return BadRequest(new { message = "nguoiMuaId invalid" });

            var data = await _repo.GetDetailAsync(donHangId, nguoiMuaId);
            if (data == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

            return Ok(data);
        }

        // POST: /api/DonHang
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaoDonHangRequest req)
        {
            if (req.NguoiMuaID <= 0) return BadRequest(new { message = "NguoiMuaID invalid" });
            if (req.SuKienID <= 0) return BadRequest(new { message = "SuKienID invalid" });
            if (req.Items == null || req.Items.Count == 0) return BadRequest(new { message = "Items required" });

            var donHangId = await _repo.CreateAsync(req);
            return Ok(new { donHangId });
        }

        // PATCH: /api/DonHang/{donHangId}/cancel?nguoiMuaId=1
        [HttpPatch("{donHangId:int}/cancel")]
        public async Task<IActionResult> Cancel([FromRoute] int donHangId, [FromQuery] int nguoiMuaId)
        {
            if (nguoiMuaId <= 0) return BadRequest(new { message = "nguoiMuaId invalid" });

            var ok = await _repo.CancelAsync(donHangId, nguoiMuaId);
            if (!ok) return BadRequest(new { message = "Không thể hủy (đơn không tồn tại hoặc trạng thái không cho phép)." });

            return Ok(new { message = "Đã hủy đơn hàng." });
        }
    }
}
