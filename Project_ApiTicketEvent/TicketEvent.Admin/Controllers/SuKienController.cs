using Microsoft.AspNetCore.Mvc;
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
