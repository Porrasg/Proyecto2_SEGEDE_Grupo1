using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    public class DistributionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
