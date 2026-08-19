using Microsoft.AspNetCore.Mvc;

namespace EcommerceSystem.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
