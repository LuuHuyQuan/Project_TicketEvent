using Microsoft.AspNetCore.Mvc;

namespace TicketEvent.Admin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
