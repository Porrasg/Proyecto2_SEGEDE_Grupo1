using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    public class AuditsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
