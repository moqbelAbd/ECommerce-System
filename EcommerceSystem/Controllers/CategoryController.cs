using EcommerceSystem.Data;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceSystem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        //get  category

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();

            return View(categories);

        }

        // GET: Category/Details

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && !c.IsDeleted);

            if (category == null)
                return NotFound();

            return View(category);
        }


        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

    }
}
