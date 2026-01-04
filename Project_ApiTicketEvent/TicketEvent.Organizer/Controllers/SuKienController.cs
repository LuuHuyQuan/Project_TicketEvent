using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using Models;

namespace TicketEvent.Organizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuKienController : ControllerBase
    {
        private readonly ISuKienRepository _suKienRepository;

        public SuKienController(ISuKienRepository suKienRepository)
        {
            _suKienRepository = suKienRepository;
        }

        // GET: api/sukien
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SuKien>>> GetAll()
        {
            try
            {
                var suKiens = await _suKienRepository.GetAllAsync();
                return Ok(suKiens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách sự kiện", error = ex.Message });
            }
        }

        // GET: api/sukien/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SuKien>> GetById(int id)
        {
            try
            {
                var suKien = await _suKienRepository.GetByIdAsync(id);

                if (suKien == null)
                {
                    return NotFound(new { message = $"Không tìm thấy sự kiện với ID: {id}" });
                }

                return Ok(suKien);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin sự kiện", error = ex.Message });
            }
        }

        // POST: api/sukien
        [HttpPost]
        public async Task<ActionResult<SuKien>> Create([FromBody] SuKien suKien)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate thời gian
                if (suKien.ThoiGianKetThuc <= suKien.ThoiGianBatDau)
                {
                    return BadRequest(new { message = "Thời gian kết thúc phải sau thời gian bắt đầu" });
                }

                // Set ngày tạo
                suKien.NgayTao = DateTime.Now;

                var newId = await _suKienRepository.CreateAsync(suKien);
                suKien.SuKienID = newId;

                return CreatedAtAction(nameof(GetById), new { id = newId }, suKien);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo sự kiện", error = ex.Message });
            }
        }

        // PUT: api/sukien/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] SuKien suKien)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != suKien.SuKienID)
                {
                    return BadRequest(new { message = "ID không khớp" });
                }

                // Kiểm tra tồn tại
                var existingSuKien = await _suKienRepository.GetByIdAsync(id);
                if (existingSuKien == null)
                {
                    return NotFound(new { message = $"Không tìm thấy sự kiện với ID: {id}" });
                }

                // Validate thời gian
                if (suKien.ThoiGianKetThuc <= suKien.ThoiGianBatDau)
                {
                    return BadRequest(new { message = "Thời gian kết thúc phải sau thời gian bắt đầu" });
                }

                var success = await _suKienRepository.UpdateAsync(suKien);

                if (!success)
                {
                    return StatusCode(500, new { message = "Không thể cập nhật sự kiện" });
                }

                return Ok(new { message = "Cập nhật sự kiện thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật sự kiện", error = ex.Message });
            }
        }
    }
}
