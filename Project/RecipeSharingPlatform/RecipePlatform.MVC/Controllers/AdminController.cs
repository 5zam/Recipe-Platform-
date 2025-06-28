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
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }



        public async Task<IActionResult> Dashboard()
        {
            // جلب جميع المستخدمين ما عدا الـ Admin
            var allUsersQuery = _userManager.Users.ToList();
            var allUsers = new List<ApplicationUser>();

            foreach (var user in allUsersQuery)
            {
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    allUsers.Add(user);
                }
            }

            // حساب الإحصائيات (بدون الـ Admin)
            var totalUsers = allUsers.Count;
            var suspendedUsers = allUsers.Count(u => !u.IsActive);

            // جلب الوصفات (بدون وصفات الـ Admin)
            var allRecipesQuery = await _context.Recipes
                .Include(r => r.Author)
                .Include(r => r.Category)
                .Where(r => r.Author.IsActive == true)
                .ToListAsync();

            var allRecipes = new List<Recipe>();
            foreach (var recipe in allRecipesQuery)
            {
                if (!await _userManager.IsInRoleAsync(recipe.Author, "Admin"))
                {
                    allRecipes.Add(recipe);
                }
            }

            var allCategories = await _context.Categories.ToListAsync();

            var viewModel = new AdminDashboardVM
            {
                TotalUsers = totalUsers, // 8 بدلاً من 9
                SuspendedUsers = suspendedUsers, // 1 بدلاً من 2 (إذا كان test محظور)
                TotalRecipes = allRecipes.Count, // بدون وصفات الـ Admin
                Users = allUsers, // ✅ بدون الـ Admin
                Recipes = allRecipes, // ✅ بدون وصفات الـ Admin
                Categories = allCategories
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.Categories.Add(new Category { Name = name.Trim() });
                await _context.SaveChangesAsync();
            }

           
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat is not null)
            {
                _context.Categories.Remove(cat);
                await _context.SaveChangesAsync();
            }

            
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> ViewRecipe(int id)
        {
            try
            {
                var recipe = await _context.Recipes
                    .Include(r => r.Author)
                    .Include(r => r.Category)
                    .Include(r => r.Ingredients)
                    .Include(r => r.Instructions)
                    .Include(r => r.Ratings)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (recipe == null)
                {
                    TempData["ErrorMessage"] = "Recipe not found.";
                    return RedirectToAction("Dashboard");
                }

                // إعادة توجيه لصفحة تفاصيل الوصفة العادية
                return RedirectToAction("Details", "Recipe", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error viewing recipe: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// حذف الوصفة (للـ Admin فقط)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            try
            {
                _logger.LogInformation($"🗑️ Admin attempting to delete recipe {id}");

                var recipe = await _context.Recipes
                    .Include(r => r.Ingredients)
                    .Include(r => r.Instructions)
                    .Include(r => r.Ratings)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (recipe == null)
                {
                    _logger.LogWarning($"❌ Recipe {id} not found for deletion");
                    TempData["ErrorMessage"] = "Recipe not found.";
                    return RedirectToAction("Dashboard");
                }

                var recipeTitle = recipe.Title;

                // حذف المكونات والتعليمات والتقييمات أولاً
                if (recipe.Ingredients.Any())
                {
                    _context.RecipeIngredients.RemoveRange(recipe.Ingredients);
                }

                if (recipe.Instructions.Any())
                {
                    _context.RecipeInstructions.RemoveRange(recipe.Instructions);
                }

                if (recipe.Ratings.Any())
                {
                    _context.Rates.RemoveRange(recipe.Ratings);
                }

                // ثم حذف الوصفة
                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Recipe '{recipeTitle}' deleted successfully by admin");
                TempData["SuccessMessage"] = $"Recipe '{recipeTitle}' has been deleted successfully.";

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error deleting recipe {id}");
                TempData["ErrorMessage"] = $"Error deleting recipe: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }

    }
}
