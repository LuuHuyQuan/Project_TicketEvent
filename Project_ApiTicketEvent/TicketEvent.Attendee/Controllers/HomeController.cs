using Microsoft.AspNetCore.Mvc;

namespace TicketEvent.Attendee.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
