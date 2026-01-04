using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories.Interfaces;

namespace TicketEvent.Admin.Controllers
{
    [Route("api/admin/diadiem")]
    [ApiController]
    public class DiaDiemAdminController : ControllerBase
    {
        private readonly IDiaDiemReponsitory _repo;

        public DiaDiemAdminController(IDiaDiemReponsitory repo)
        {
            _repo = repo;
        }
     
        [HttpPost]
        public IActionResult Create([FromBody] DiaDiem model)
        {
            model.TrangThai = true;

            var newId = _repo.Create(model);
            return Ok(new { success = true, message = "Tạo địa điểm thành công", id = newId });
        }
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] DiaDiem model)
        {
            model.DiaDiemID = id;

            var ok = _repo.Update(model);
            if (!ok)
                return Ok(new { success = false, message = "Cập nhật thất bại" });

            return Ok(new { success = true, message = "Cập nhật thành công" });
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var ok = _repo.Delete(id);
            if (!ok)
                return Ok(new { success = false, message = "Xóa thất bại" });

            return Ok(new { success = true, message = "Xóa thành công" });
        }
    }
}
