using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    public class FailuresController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
