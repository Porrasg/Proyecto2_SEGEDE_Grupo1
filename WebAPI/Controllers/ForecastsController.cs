using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    public class ForecastsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
