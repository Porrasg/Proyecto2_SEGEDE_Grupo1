using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    public class InvoicesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
