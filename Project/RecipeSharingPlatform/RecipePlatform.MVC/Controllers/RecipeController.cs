using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipePlatform.BLL.Interfaces;
using RecipePlatform.DAL.Context;
using RecipePlatform.Models.ApplicationModels;
using RecipePlatform.Models.UserModels;
using RecipePlatform.MVC.Models.ViewModels;

namespace RecipePlatform.MVC.Controllers
{
    [Authorize]
    public class RecipeController : Controller
    {
        private readonly IGenericRepository<Recipe> _recipeRepo;
        private readonly IRecipeService _recipeService;
        private readonly UserManager<ApplicationUser> _userManager;
        

        public RecipeController(IGenericRepository<Recipe> recipeRepo,
                                UserManager<ApplicationUser> userManager,
                                IRecipeService recipeService)
        {
            _recipeRepo = recipeRepo;
            _userManager = userManager;
            _recipeService = recipeService;
        }

        public async Task<IActionResult> MyRecipes()
        {
            var userId = _userManager.GetUserId(User);
            var recipes = await _recipeService.GetRecipesByUserId(userId);

            var viewModel = new MyRecipesVM
            {
                Recipes = recipes
            };

            return View(viewModel);
        }

    }
}
