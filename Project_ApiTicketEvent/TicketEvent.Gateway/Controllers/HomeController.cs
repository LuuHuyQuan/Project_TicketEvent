using Microsoft.AspNetCore.Mvc;

namespace TicketEvent.Gateway.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
