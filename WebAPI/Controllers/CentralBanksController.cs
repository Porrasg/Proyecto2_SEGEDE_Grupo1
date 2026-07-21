using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    public class CentralBanksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
