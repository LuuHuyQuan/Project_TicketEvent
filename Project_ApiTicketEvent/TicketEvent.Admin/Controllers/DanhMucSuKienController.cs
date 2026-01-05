using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace TicketEvent.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhMucSuKienController : ControllerBase
    {
        private readonly IDanhMucSuKienRepository _repo;
        private readonly IDanhMucSuKienService _service;

        public DanhMucSuKienController(IDanhMucSuKienRepository repo, IDanhMucSuKienService service)
        {
            _repo = repo;
            _service = service;

        }
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
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(new { success = true, data = _repo.GetAllAsync() });
        }

        [HttpPost]
        public IActionResult Create(DanhMucSuKien model)
        {
            model.TrangThai = true;
            var id = _repo.Create(model);
            return Ok(new { success = true, id });
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, DanhMucSuKien model)
        {
            model.DanhMucID = id;
            return Ok(new { success = _repo.Update(model) });
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            return Ok(new { success = _repo.Delete(id) });
        }
    }
}
