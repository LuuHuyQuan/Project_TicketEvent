using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace TicketEvent.Admin.Controllers
{
    [Route("api/admin/diadiem")]
    [ApiController]
    public class DiaDiemAdminController : ControllerBase
    {
        private readonly IDiaDiemService _service;

        public DiaDiemAdminController(IDiaDiemService service)
        {
            _service = service;
        }
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
        [HttpPost]
        public IActionResult Create([FromBody] DiaDiem model)
        {
            if (model == null) return BadRequest(new { success = false, message = "Body rỗng." });

            try
            {
                // Service sẽ tự set TrangThai = true (nếu bạn làm theo cách 1 mình hướng dẫn)
                var newId = _service.Create(model);
                return CreatedAtAction(nameof(Update), new { id = newId },
                    new { success = true, message = "Tạo địa điểm thành công", id = newId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] DiaDiem model)
        {
            if (model == null) return BadRequest(new { success = false, message = "Body rỗng." });
            if (id <= 0) return BadRequest(new { success = false, message = "Id không hợp lệ." });

            // ép id theo route
            model.DiaDiemID = id;

            try
            {
                var ok = _service.Update(model);
                if (!ok) return NotFound(new { success = false, message = "Không tìm thấy địa điểm để cập nhật." });

                return Ok(new { success = true, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (id <= 0) return BadRequest(new { success = false, message = "Id không hợp lệ." });

            try
            {
                var ok = _service.Delete(id);
                if (!ok) return NotFound(new { success = false, message = "Không tìm thấy địa điểm để xóa." });

                return Ok(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
