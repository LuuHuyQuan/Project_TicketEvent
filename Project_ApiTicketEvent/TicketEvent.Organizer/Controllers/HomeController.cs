using Microsoft.AspNetCore.Mvc;

namespace TicketEvent.Organizer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
