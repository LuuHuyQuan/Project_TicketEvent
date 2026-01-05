using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.Interfaces;

namespace TicketEvent.Organizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckinController : ControllerBase
    {
        private readonly ICheckinRepository _checkinRepository;

        public CheckinController(ICheckinRepository checkinRepository)
        {
            _checkinRepository = checkinRepository;
        }

        // GET: api/checkin
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NhatKyCheckIn>>> GetAll()
        {
            try
            {
                var checkins = await _checkinRepository.GetAllAsync();
                return Ok(checkins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách check-in", error = ex.Message });
            }
        }

        // GET: api/checkin/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NhatKyCheckIn>> GetById(int id)
        {
            try
            {
                var checkin = await _checkinRepository.GetByIdAsync(id);

                if (checkin == null)
                {
                    return NotFound(new { message = $"Không tìm thấy check-in với ID: {id}" });
                }

                return Ok(checkin);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin check-in", error = ex.Message });
            }
        }

        // GET: api/checkin/sukien/1
        [HttpGet("sukien/{suKienId}")]
        public async Task<ActionResult<IEnumerable<NhatKyCheckIn>>> GetBySuKien(int suKienId)
        {
            try
            {
                var checkins = await _checkinRepository.GetBySuKienAsync(suKienId);
                return Ok(checkins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách check-in theo sự kiện", error = ex.Message });
            }
        }

        // GET: api/checkin/ve/1
        [HttpGet("ve/{veId}")]
        public async Task<ActionResult<IEnumerable<NhatKyCheckIn>>> GetByVe(int veId)
        {
            try
            {
                var checkins = await _checkinRepository.GetByVeAsync(veId);
                return Ok(checkins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách check-in theo vé", error = ex.Message });
            }
        }

        // GET: api/checkin/sukien/1/total
        [HttpGet("sukien/{suKienId}/total")]
        public async Task<ActionResult<object>> GetTotalBySuKien(int suKienId)
        {
            try
            {
                var total = await _checkinRepository.GetTotalCheckinBySuKienAsync(suKienId);
                return Ok(new { suKienId = suKienId, totalCheckin = total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi đếm số lượng check-in", error = ex.Message });
            }
        }

        // POST: api/checkin
        [HttpPost]
        public async Task<ActionResult<NhatKyCheckIn>> Create([FromBody] NhatKyCheckIn checkin)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Set thời gian check-in là thời gian hiện tại
                checkin.ThoiGianCheckin = DateTime.Now;

                var newId = await _checkinRepository.CreateAsync(checkin);
                checkin.CheckinID = newId;

                return CreatedAtAction(nameof(GetById), new { id = newId }, checkin);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo check-in", error = ex.Message });
            }
        }

        // PUT: api/checkin/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] NhatKyCheckIn checkin)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != checkin.CheckinID)
                {
                    return BadRequest(new { message = "ID không khớp" });
                }

                // Kiểm tra tồn tại
                var existingCheckin = await _checkinRepository.GetByIdAsync(id);
                if (existingCheckin == null)
                {
                    return NotFound(new { message = $"Không tìm thấy check-in với ID: {id}" });
                }

                var success = await _checkinRepository.UpdateAsync(checkin);

                if (!success)
                {
                    return StatusCode(500, new { message = "Không thể cập nhật check-in" });
                }

                return Ok(new { message = "Cập nhật check-in thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật check-in", error = ex.Message });
            }
        }
    }
}
