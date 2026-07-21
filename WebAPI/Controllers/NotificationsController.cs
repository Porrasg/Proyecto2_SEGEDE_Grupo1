using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    public class NotificationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
