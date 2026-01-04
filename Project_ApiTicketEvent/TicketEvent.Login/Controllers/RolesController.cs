using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace TicketEvent.Login.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IVaiTroRepository _roleRepo;

        public RolesController(IVaiTroRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }


        [HttpGet]
        public IActionResult GetAll()
        {
            var roles = _roleRepo.GetAll();
            return Ok(roles);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var role = _roleRepo.GetById(id);
            if (role == null) return NotFound();

            return Ok(role);
        }
    }
}
