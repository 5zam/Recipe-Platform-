using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipePlatform.DAL.Context;
using RecipePlatform.Models.ApplicationModels;
using RecipePlatform.Models.UserModels;
using RecipePlatform.MVC.Models.ViewModels;

namespace RecipePlatform.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager,
                                           ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var vm = new AdminDashboardVM
            {
                Users = _context.Users.ToList(),
                Recipes = _context.Recipes
                             .Include(r => r.Author)
                             .Include(r => r.Category)
                             .ToList(),
                Categories = _context.Categories.ToList()
            };
            return View(vm);
        }

        public async Task<IActionResult> Dashboard()
        {
            var users = _userManager.Users.ToList();
            var recipes = _context.Recipes.Include(r => r.Author).Include(r => r.Category).ToList();
            var categories = _context.Categories.ToList();

            var viewModel = new AdminDashboardVM
            {
                Users = users,
                Recipes = recipes,
                Categories = categories,
                TotalUsers = users.Count,
                SuspendedUsers = users.Count(u => !u.IsActive),
                TotalRecipes = recipes.Count
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSuspend(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return RedirectToAction("Dashboard");
        }

        //view
        public async Task<IActionResult> ViewUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            
            return View(user);
        }


        //add category 
        [HttpPost]
        public async Task<IActionResult> AddCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return RedirectToAction("Dashboard");

            _context.Categories.Add(new Category { Name = name });
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
        //[HttpPost, ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddCategory(string name)
        //{
        //    if (!string.IsNullOrWhiteSpace(name))
        //    {
        //        _context.Categories.Add(new Category { Name = name.Trim() });
        //        await _context.SaveChangesAsync();
        //    }
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat is not null)
            {
                _context.Categories.Remove(cat);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


    }
}
