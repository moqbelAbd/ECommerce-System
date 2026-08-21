using Microsoft.AspNetCore.Mvc;

namespace EcommerceSystem.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Cart()
        {
            return View();
        }
            public IActionResult OrderHistory()
            {
                return View();
            }

            public IActionResult OrderConfirmation()
            {
                return View();
            }

            public IActionResult Wishlist()
            {
                return View();
            }

            public IActionResult Checkout()
            {
                return View();
            }
        public IActionResult OrderDetails(int id)
        { 
            return View();
        }
    }
    }
