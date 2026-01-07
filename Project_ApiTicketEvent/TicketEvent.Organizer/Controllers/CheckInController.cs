using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Requests;
using Repositories.Interfaces;

namespace TicketEvent.Organizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInRepository _repo;

        public CheckInController(ICheckInRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        public IActionResult Post([FromBody] CheckInRequest req)
        {
            var result = _repo.Checkin(req);
            return Ok(result);
        }
    }
}
