using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RecipePlatform.BLL.Interfaces;
using RecipePlatform.Models.ApplicationModels.Enums;
using RecipePlatform.Models.ApplicationModels;
using RecipePlatform.Models.UserModels;
using RecipePlatform.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace RecipePlatform.MVC.Controllers
{
    [AllowAnonymous]
    public class RecipeController : Controller
    {
        private readonly IGenericRepository<Recipe> _recipeRepo;
        private readonly IGenericRepository<Category> _catRepo;
        private readonly IRecipeService _recipeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RecipeController> _logger;
        private readonly IRatingService _ratingService;

        public RecipeController(
            IGenericRepository<Recipe> recipeRepo,
            UserManager<ApplicationUser> userManager,
            IRecipeService recipeService,
            IGenericRepository<Category> catRepo,
            ILogger<RecipeController> logger,
            IRatingService ratingService)
        {
            _recipeRepo = recipeRepo;
            _userManager = userManager;
            _recipeService = recipeService;
            _catRepo = catRepo;
            _logger = logger;
            _ratingService = ratingService;
        }

        // EXISTING METHODS (unchanged)

        [Authorize]
        public async Task<IActionResult> MyRecipes()
        {
            try
            {
                _logger.LogInformation("🏠 MyRecipes method called");
                var userId = _userManager.GetUserId(User);
                _logger.LogInformation($"User ID: {userId ?? "NULL"}");

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var recipes = await _recipeService.GetRecipesByUserId(userId);
                _logger.LogInformation($"Found {recipes?.Count ?? 0} recipes");

                var user = await _userManager.GetUserAsync(User);

                
                var displayName = string.IsNullOrEmpty(user?.UserName)
                    ? user?.Email?.Split('@')[0] ?? "User"
                    : user.UserName;

                var viewModel = new MyRecipesVM
                {
                    Recipes = recipes ?? new List<Recipe>(),
                    UserName = displayName
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in MyRecipes");
                return View(new MyRecipesVM { Recipes = new List<Recipe>(), UserName = "User" });
            }
        }

        public IActionResult Index()
        {
            _logger.LogInformation("📍 Index method called - redirecting to MyRecipes");
            return RedirectToAction("MyRecipes");
        }

        // GET: Recipe/Add
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Add()
        {
            try
            {
                _logger.LogInformation("✅ GET Add method called");

                var viewModel = new AddRecipeVM();

                var categories = await _catRepo.GetAllAsync();
                _logger.LogInformation($"Categories loaded: {categories?.Count() ?? 0}");

                viewModel.Categories = categories?.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }) ?? new List<SelectListItem>();

                viewModel.Difficulties = Enum.GetValues(typeof(DifficultyLevel))
                    .Cast<DifficultyLevel>()
                    .Select(d => new SelectListItem
                    {
                        Value = d.ToString(),
                        Text = GetDifficultyDisplayName(d)
                    });

                _logger.LogInformation($"Difficulties loaded: {viewModel.Difficulties?.Count() ?? 0}");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in GET Add method");
                return Content($"Error loading Add page: {ex.Message}");
            }
        }

        // POST: Recipe/Add
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddRecipeVM viewModel)
        {
            try
            {
                _logger.LogInformation("🚀 POST Add method called");

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Remove unnecessary model state entries
                ModelState.Remove("Recipe.Category");
                ModelState.Remove("Recipe.Author");
                ModelState.Remove("Recipe.Ratings");

                // Clean up ingredients and instructions
                viewModel.IngredientsList = viewModel.IngredientsList?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList() ?? new List<string>();

                viewModel.InstructionsList = viewModel.InstructionsList?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList() ?? new List<string>();

                // Validate ingredients and instructions
                if (!viewModel.IngredientsList.Any())
                {
                    ModelState.AddModelError("IngredientsList", "At least one ingredient is required");
                }

                if (!viewModel.InstructionsList.Any())
                {
                    ModelState.AddModelError("InstructionsList", "At least one instruction is required");
                }

                if (!ModelState.IsValid)
                {
                    await ReloadDropdownData(viewModel);
                    return View(viewModel);
                }

                // Prepare recipe data
                viewModel.Recipe.AuthorId = userId;
                viewModel.Recipe.CreatedDate = DateTime.Now;

                // Create ingredients
                var ingredients = new List<RecipeIngredient>();
                for (int i = 0; i < viewModel.IngredientsList.Count; i++)
                {
                    ingredients.Add(new RecipeIngredient
                    {
                        Name = viewModel.IngredientsList[i].Trim(),
                        Order = i + 1
                    });
                }

                // Create instructions
                var instructions = new List<RecipeInstruction>();
                for (int i = 0; i < viewModel.InstructionsList.Count; i++)
                {
                    instructions.Add(new RecipeInstruction
                    {
                        Description = viewModel.InstructionsList[i].Trim(),
                        StepNumber = i + 1
                    });
                }

                viewModel.Recipe.Ingredients = ingredients;
                viewModel.Recipe.Instructions = instructions;

                await _recipeRepo.AddAsync(viewModel.Recipe);

                _logger.LogInformation("✅ Recipe saved successfully!");
                TempData["SuccessMessage"] = "Recipe added successfully! 🎉";

                return RedirectToAction("MyRecipes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in POST Add method");
                ModelState.AddModelError("", $"An error occurred while saving the recipe: {ex.Message}");

                if (viewModel != null)
                {
                    await ReloadDropdownData(viewModel);
                    return View(viewModel);
                }

                return Content($"Critical Error: {ex.Message}");
            }
        }

        // NEW: GET Edit Recipe
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditRecipe(int id)
        {
            try
            {
                _logger.LogInformation($"✏️ GET EditRecipe called for recipe {id}");

                var recipe = await _recipeService.GetRecipeWithRatingsAsync(id);
                if (recipe == null)
                {
                    _logger.LogWarning($"❌ Recipe {id} not found");
                    return NotFound();
                }

                var userId = _userManager.GetUserId(User);
                if (recipe.AuthorId != userId)
                {
                    _logger.LogWarning($"❌ User {userId} trying to edit recipe {id} not owned by them");
                    TempData["ErrorMessage"] = "You can only edit your own recipes.";
                    return RedirectToAction("MyRecipes");
                }

                var categories = await _catRepo.GetAllAsync();

                var viewModel = new EditRecipeVM
                {
                    Recipe = recipe,
                    IngredientsList = recipe.Ingredients?.OrderBy(i => i.Order).Select(i => i.Name).ToList() ?? new List<string>(),
                    InstructionsList = recipe.Instructions?.OrderBy(i => i.StepNumber).Select(i => i.Description).ToList() ?? new List<string>(),
                    Categories = categories?.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name,
                        Selected = c.Id == recipe.CategoryId
                    }) ?? new List<SelectListItem>(),
                    Difficulties = Enum.GetValues(typeof(DifficultyLevel))
                        .Cast<DifficultyLevel>()
                        .Select(d => new SelectListItem
                        {
                            Value = d.ToString(),
                            Text = GetDifficultyDisplayName(d),
                            Selected = d == recipe.Difficulty
                        })
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error in GET EditRecipe for recipe {id}");
                return Content($"Error loading recipe for editing: {ex.Message}");
            }
        }

        // NEW: POST Edit Recipe
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRecipe(EditRecipeVM viewModel)
        {
            try
            {
                _logger.LogInformation($"🚀 POST EditRecipe called for recipe {viewModel.Recipe?.Id}");

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var existingRecipe = await _recipeService.GetRecipeWithRatingsAsync(viewModel.Recipe.Id);
                if (existingRecipe == null)
                {
                    return NotFound();
                }

                if (existingRecipe.AuthorId != userId)
                {
                    TempData["ErrorMessage"] = "You can only edit your own recipes.";
                    return RedirectToAction("MyRecipes");
                }

                // Remove unnecessary model state entries
                ModelState.Remove("Recipe.Category");
                ModelState.Remove("Recipe.Author");
                ModelState.Remove("Recipe.Ratings");

                // Clean up ingredients and instructions
                viewModel.IngredientsList = viewModel.IngredientsList?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList() ?? new List<string>();

                viewModel.InstructionsList = viewModel.InstructionsList?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList() ?? new List<string>();

                // Validate ingredients and instructions
                if (!viewModel.IngredientsList.Any())
                {
                    ModelState.AddModelError("IngredientsList", "At least one ingredient is required");
                }

                if (!viewModel.InstructionsList.Any())
                {
                    ModelState.AddModelError("InstructionsList", "At least one instruction is required");
                }

                if (!ModelState.IsValid)
                {
                    await ReloadEditDropdownData(viewModel);
                    return View(viewModel);
                }

                // Update recipe properties
                existingRecipe.Title = viewModel.Recipe.Title;
                existingRecipe.Description = viewModel.Recipe.Description;
                existingRecipe.CategoryId = viewModel.Recipe.CategoryId;
                existingRecipe.Difficulty = viewModel.Recipe.Difficulty;
                existingRecipe.PrepTimeMinutes = viewModel.Recipe.PrepTimeMinutes;
                existingRecipe.CookTimeMinutes = viewModel.Recipe.CookTimeMinutes;
                existingRecipe.Servings = viewModel.Recipe.Servings;

                // Update ingredients (remove old, add new)
                existingRecipe.Ingredients.Clear();
                for (int i = 0; i < viewModel.IngredientsList.Count; i++)
                {
                    existingRecipe.Ingredients.Add(new RecipeIngredient
                    {
                        Name = viewModel.IngredientsList[i].Trim(),
                        Order = i + 1,
                        RecipeId = existingRecipe.Id
                    });
                }

                // Update instructions (remove old, add new)
                existingRecipe.Instructions.Clear();
                for (int i = 0; i < viewModel.InstructionsList.Count; i++)
                {
                    existingRecipe.Instructions.Add(new RecipeInstruction
                    {
                        Description = viewModel.InstructionsList[i].Trim(),
                        StepNumber = i + 1,
                        RecipeId = existingRecipe.Id
                    });
                }

                await _recipeRepo.UpdateAsync(existingRecipe);

                _logger.LogInformation($"✅ Recipe {existingRecipe.Id} updated successfully!");
                TempData["SuccessMessage"] = "Recipe updated successfully! ✏️";

                return RedirectToAction("MyRecipes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error in POST EditRecipe for recipe {viewModel.Recipe?.Id}");
                ModelState.AddModelError("", $"An error occurred while updating the recipe: {ex.Message}");

                await ReloadEditDropdownData(viewModel);
                return View(viewModel);
            }
        }

        // NEW: DELETE Recipe
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            try
            {
                _logger.LogInformation($"🗑️ DeleteRecipe called for recipe {id}");

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var recipe = await _recipeService.GetByIdAsync(id);
                if (recipe == null)
                {
                    _logger.LogWarning($"❌ Recipe {id} not found");
                    TempData["ErrorMessage"] = "Recipe not found.";
                    return RedirectToAction("MyRecipes");
                }

                if (recipe.AuthorId != userId)
                {
                    _logger.LogWarning($"❌ User {userId} trying to delete recipe {id} not owned by them");
                    TempData["ErrorMessage"] = "You can only delete your own recipes.";
                    return RedirectToAction("MyRecipes");
                }

                await _recipeRepo.DeleteAsync(recipe);

                _logger.LogInformation($"✅ Recipe {id} deleted successfully!");
                TempData["SuccessMessage"] = "Recipe deleted successfully! 🗑️";

                return RedirectToAction("MyRecipes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error deleting recipe {id}");
                TempData["ErrorMessage"] = $"An error occurred while deleting the recipe: {ex.Message}";
                return RedirectToAction("MyRecipes");
            }
        }

        // EXISTING Details method (unchanged)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation($"📖 Details method called for recipe {id}");

                var recipe = await _recipeService.GetRecipeWithRatingsAsync(id);
                if (recipe == null)
                {
                    _logger.LogWarning($"❌ Recipe {id} not found");
                    return NotFound();
                }

                var userId = _userManager.GetUserId(User);
                _logger.LogInformation($"Current User ID: {userId ?? "NULL"}");

                var viewModel = new RecipeRatingViewModel
                {
                    Recipe = recipe,
                    AverageRating = await _recipeService.GetAverageRating(id),
                    TotalRatings = await _recipeService.GetTotalRatingsCount(id),
                    IsUserLoggedIn = !string.IsNullOrEmpty(userId)
                };

                if (viewModel.IsUserLoggedIn)
                {
                    viewModel.HasUserRated = await _recipeService.HasUserRated(id, userId);
                    if (viewModel.HasUserRated)
                    {
                        var userRating = await _recipeService.GetUserRating(id, userId);
                        viewModel.UserRating = userRating?.RateValue;
                    }
                }

                _logger.LogInformation($"✅ Recipe details loaded - Avg Rating: {viewModel.AverageRating}, Total: {viewModel.TotalRatings}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error loading recipe details for {id}");
                return Content($"Error loading recipe: {ex.Message}");
            }
        }

        // HELPER METHODS

        private async Task ReloadDropdownData(AddRecipeVM viewModel)
        {
            try
            {
                _logger.LogInformation("🔄 Reloading dropdown data");

                var categories = await _catRepo.GetAllAsync();
                viewModel.Categories = categories?.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }) ?? new List<SelectListItem>();

                viewModel.Difficulties = Enum.GetValues(typeof(DifficultyLevel))
                    .Cast<DifficultyLevel>()
                    .Select(d => new SelectListItem
                    {
                        Value = d.ToString(),
                        Text = GetDifficultyDisplayName(d)
                    });

                _logger.LogInformation($"Reloaded {viewModel.Categories.Count()} categories and {viewModel.Difficulties.Count()} difficulties");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error reloading dropdown data");
                viewModel.Categories = new List<SelectListItem>();
                viewModel.Difficulties = new List<SelectListItem>();
            }
        }

        private async Task ReloadEditDropdownData(EditRecipeVM viewModel)
        {
            try
            {
                var categories = await _catRepo.GetAllAsync();
                viewModel.Categories = categories?.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == viewModel.Recipe.CategoryId
                }) ?? new List<SelectListItem>();

                viewModel.Difficulties = Enum.GetValues(typeof(DifficultyLevel))
                    .Cast<DifficultyLevel>()
                    .Select(d => new SelectListItem
                    {
                        Value = d.ToString(),
                        Text = GetDifficultyDisplayName(d),
                        Selected = d == viewModel.Recipe.Difficulty
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error reloading edit dropdown data");
                viewModel.Categories = new List<SelectListItem>();
                viewModel.Difficulties = new List<SelectListItem>();
            }
        }

        private string GetDifficultyDisplayName(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => "Easy",
                DifficultyLevel.Medium => "Medium",
                DifficultyLevel.Hard => "Hard",
                _ => difficulty.ToString()
            };
        }

        // TEST METHODS (can be removed in production)
        [HttpGet]
        public IActionResult TestGet()
        {
            _logger.LogInformation("✅ TestGet method called");
            return Content("TestGet works!");
        }

        [HttpPost]
        public IActionResult TestPost(string test)
        {
            _logger.LogInformation($"✅ TestPost method called with value: {test ?? "NULL"}");
            return Content($"TestPost received: {test}");
        }
    }
}