using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.Interfaces;

namespace TicketEvent.Organizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiVeController : ControllerBase
    {
        private readonly ILoaiVeRepository _loaiVeRepository;

        public LoaiVeController(ILoaiVeRepository loaiVeRepository)
        {
            _loaiVeRepository = loaiVeRepository;
        }

        // GET: api/loaive
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoaiVe>>> GetAll()
        {
            try
            {
                var loaiVes = await _loaiVeRepository.GetAllAsync();
                return Ok(loaiVes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách loại vé", error = ex.Message });
            }
        }

        // GET: api/loaive/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LoaiVe>> GetById(int id)
        {
            try
            {
                var loaiVe = await _loaiVeRepository.GetByIdAsync(id);

                if (loaiVe == null)
                {
                    return NotFound(new { message = $"Không tìm thấy loại vé với ID: {id}" });
                }

                return Ok(loaiVe);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin loại vé", error = ex.Message });
            }
        }

        // GET: api/loaive/sukien/1
        [HttpGet("sukien/{suKienId}")]
        public async Task<ActionResult<IEnumerable<LoaiVe>>> GetBySuKien(int suKienId)
        {
            try
            {
                var loaiVes = await _loaiVeRepository.GetBySuKienAsync(suKienId);
                return Ok(loaiVes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách loại vé theo sự kiện", error = ex.Message });
            }
        }

        // POST: api/loaive
        [HttpPost]
        public async Task<ActionResult<LoaiVe>> Create([FromBody] LoaiVe loaiVe)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate số lượng
                if (loaiVe.SoLuongDaBan > loaiVe.SoLuongToiDa)
                {
                    return BadRequest(new { message = "Số lượng đã bán không được lớn hơn số lượng tối đa" });
                }

                // Validate thời gian mở bán và đóng bán
                if (loaiVe.ThoiGianMoBan.HasValue && loaiVe.ThoiGianDongBan.HasValue)
                {
                    if (loaiVe.ThoiGianDongBan <= loaiVe.ThoiGianMoBan)
                    {
                        return BadRequest(new { message = "Thời gian đóng bán phải sau thời gian mở bán" });
                    }
                }

                // Validate giá
                if (loaiVe.DonGia < 0)
                {
                    return BadRequest(new { message = "Đơn giá không được âm" });
                }

                var newId = await _loaiVeRepository.CreateAsync(loaiVe);
                loaiVe.LoaiVeID = newId;

                return CreatedAtAction(nameof(GetById), new { id = newId }, loaiVe);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo loại vé", error = ex.Message });
            }
        }

        // PUT: api/loaive/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] LoaiVe loaiVe)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != loaiVe.LoaiVeID)
                {
                    return BadRequest(new { message = "ID không khớp" });
                }

                // Kiểm tra tồn tại
                var existingLoaiVe = await _loaiVeRepository.GetByIdAsync(id);
                if (existingLoaiVe == null)
                {
                    return NotFound(new { message = $"Không tìm thấy loại vé với ID: {id}" });
                }

                // Validate số lượng
                if (loaiVe.SoLuongDaBan > loaiVe.SoLuongToiDa)
                {
                    return BadRequest(new { message = "Số lượng đã bán không được lớn hơn số lượng tối đa" });
                }

                // Validate thời gian
                if (loaiVe.ThoiGianMoBan.HasValue && loaiVe.ThoiGianDongBan.HasValue)
                {
                    if (loaiVe.ThoiGianDongBan <= loaiVe.ThoiGianMoBan)
                    {
                        return BadRequest(new { message = "Thời gian đóng bán phải sau thời gian mở bán" });
                    }
                }

                // Validate giá
                if (loaiVe.DonGia < 0)
                {
                    return BadRequest(new { message = "Đơn giá không được âm" });
                }

                var success = await _loaiVeRepository.UpdateAsync(loaiVe);

                if (!success)
                {
                    return StatusCode(500, new { message = "Không thể cập nhật loại vé" });
                }

                return Ok(new { message = "Cập nhật loại vé thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật loại vé", error = ex.Message });
            }
        }
    }
}
