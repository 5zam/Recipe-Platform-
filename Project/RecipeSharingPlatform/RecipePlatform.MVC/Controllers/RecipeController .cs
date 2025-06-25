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

        public RecipeController(
            IGenericRepository<Recipe> recipeRepo,
            UserManager<ApplicationUser> userManager,
            IRecipeService recipeService,
            IGenericRepository<Category> catRepo,
            ILogger<RecipeController> logger)
        {
            _recipeRepo = recipeRepo;
            _userManager = userManager;
            _recipeService = recipeService;
            _catRepo = catRepo;
            _logger = logger;
        }

        public async Task<IActionResult> MyRecipes()
        {
            try
            {
                _logger.LogInformation("🏠 MyRecipes method called");
                var userId = _userManager.GetUserId(User);
                _logger.LogInformation($"User ID: {userId ?? "NULL"}");

                var recipes = await _recipeService.GetRecipesByUserId(userId);
                _logger.LogInformation($"Found {recipes?.Count ?? 0} recipes");

                var viewModel = new MyRecipesVM
                {
                    Recipes = recipes ?? new List<Recipe>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in MyRecipes");
                return View(new MyRecipesVM { Recipes = new List<Recipe>() });
            }
        }

        public IActionResult Index()
        {
            _logger.LogInformation("📍 Index method called - redirecting to MyRecipes");
            return RedirectToAction("MyRecipes");
        }

        // GET: Recipe/Add
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            try
            {
                _logger.LogInformation("✅ GET Add method called");

                var viewModel = new AddRecipeVM();

                // تحضير قائمة التصنيفات
                var categories = await _catRepo.GetAllAsync();
                _logger.LogInformation($"Categories loaded: {categories?.Count() ?? 0}");

                viewModel.Categories = categories?.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }) ?? new List<SelectListItem>();

                // تحضير قائمة مستويات الصعوبة
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
        public async Task<IActionResult> Add(AddRecipeVM viewModel)
        {
            try
            {
                _logger.LogInformation("🚀 POST Add method called");
                _logger.LogInformation($"Model received - Recipe Title: {viewModel?.Recipe?.Title ?? "NULL"}");
                _logger.LogInformation($"Model State Valid: {ModelState.IsValid}");

                // تسجيل تفاصيل الموديل
                if (viewModel?.Recipe != null)
                {
                    _logger.LogInformation($"Recipe Details:");
                    _logger.LogInformation($"  - Title: {viewModel.Recipe.Title}");
                    _logger.LogInformation($"  - Description: {viewModel.Recipe.Description}");
                    _logger.LogInformation($"  - CategoryId: {viewModel.Recipe.CategoryId}");
                    _logger.LogInformation($"  - Difficulty: {viewModel.Recipe.Difficulty}");
                    _logger.LogInformation($"  - PrepTime: {viewModel.Recipe.PrepTimeMinutes}");
                    _logger.LogInformation($"  - CookTime: {viewModel.Recipe.CookTimeMinutes}");
                }

                // تسجيل المكونات والخطوات
                _logger.LogInformation($"Ingredients received: {viewModel?.IngredientsList?.Count ?? 0}");
                if (viewModel?.IngredientsList != null)
                {
                    for (int i = 0; i < viewModel.IngredientsList.Count; i++)
                    {
                        _logger.LogInformation($"  Ingredient {i + 1}: '{viewModel.IngredientsList[i]}'");
                    }
                }

                _logger.LogInformation($"Instructions received: {viewModel?.InstructionsList?.Count ?? 0}");
                if (viewModel?.InstructionsList != null)
                {
                    for (int i = 0; i < viewModel.InstructionsList.Count; i++)
                    {
                        _logger.LogInformation($"  Instruction {i + 1}: '{viewModel.InstructionsList[i]}'");
                    }
                }

                // تسجيل أخطاء الموديل
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("❌ ModelState is not valid!");
                    foreach (var error in ModelState)
                    {
                        if (error.Value.Errors.Any())
                        {
                            _logger.LogWarning($"Field '{error.Key}' has errors:");
                            foreach (var errorMsg in error.Value.Errors)
                            {
                                _logger.LogWarning($"  - {errorMsg.ErrorMessage}");
                            }
                        }
                    }
                }

                // التحقق من البيانات المطلوبة
                if (viewModel == null)
                {
                    _logger.LogError("❌ ViewModel is null");
                    return BadRequest("ViewModel is null");
                }

                if (viewModel.Recipe == null)
                {
                    _logger.LogError("❌ Recipe is null");
                    viewModel.Recipe = new Recipe();
                }

                // إزالة المكونات والخطوات الفارغة
                viewModel.IngredientsList = viewModel.IngredientsList?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList() ?? new List<string>();

                viewModel.InstructionsList = viewModel.InstructionsList?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList() ?? new List<string>();

                _logger.LogInformation($"After cleanup - Ingredients: {viewModel.IngredientsList.Count}, Instructions: {viewModel.InstructionsList.Count}");

                // التحقق من وجود مكونات وخطوات
                if (!viewModel.IngredientsList.Any())
                {
                    ModelState.AddModelError("IngredientsList", "يجب إضافة مكون واحد على الأقل");
                    _logger.LogWarning("❌ No ingredients provided");
                }

                if (!viewModel.InstructionsList.Any())
                {
                    ModelState.AddModelError("InstructionsList", "يجب إضافة خطوة واحدة على الأقل");
                    _logger.LogWarning("❌ No instructions provided");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("❌ Returning to view due to validation errors");
                    await ReloadDropdownData(viewModel);
                    return View(viewModel);
                }

                // إعداد بيانات الوصفة
                _logger.LogInformation("✅ Validation passed, preparing recipe data");

                var userId = _userManager.GetUserId(User);
                _logger.LogInformation($"Current User ID: {userId ?? "NULL"}");

                viewModel.Recipe.AuthorId = userId ?? "default-user"; // للاختبار
                viewModel.Recipe.CreatedDate = DateTime.Now;

                // إنشاء المكونات
                var ingredients = new List<RecipeIngredient>();
                for (int i = 0; i < viewModel.IngredientsList.Count; i++)
                {
                    var ingredient = new RecipeIngredient
                    {
                        Name = viewModel.IngredientsList[i].Trim(),
                        Order = i + 1
                    };
                    ingredients.Add(ingredient);
                    _logger.LogInformation($"Created ingredient {i + 1}: '{ingredient.Name}'");
                }

                // إنشاء الخطوات
                var instructions = new List<RecipeInstruction>();
                for (int i = 0; i < viewModel.InstructionsList.Count; i++)
                {
                    var instruction = new RecipeInstruction
                    {
                        Description = viewModel.InstructionsList[i].Trim(),
                        StepNumber = i + 1
                    };
                    instructions.Add(instruction);
                    _logger.LogInformation($"Created instruction {i + 1}: '{instruction.Description}'");
                }

                // ربط البيانات
                viewModel.Recipe.Ingredients = ingredients;
                viewModel.Recipe.Instructions = instructions;

                _logger.LogInformation("📝 Final recipe data:");
                _logger.LogInformation($"  - Title: {viewModel.Recipe.Title}");
                _logger.LogInformation($"  - CategoryId: {viewModel.Recipe.CategoryId}");
                _logger.LogInformation($"  - AuthorId: {viewModel.Recipe.AuthorId}");
                _logger.LogInformation($"  - Ingredients Count: {ingredients.Count}");
                _logger.LogInformation($"  - Instructions Count: {instructions.Count}");

                // الحفظ في قاعدة البيانات
                _logger.LogInformation("💾 Attempting to save to database...");
                await _recipeRepo.AddAsync(viewModel.Recipe);

                _logger.LogInformation("✅ Recipe saved successfully!");
                TempData["SuccessMessage"] = "تمت إضافة الوصفة بنجاح! 🎉";

                return RedirectToAction("MyRecipes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in POST Add method");
                _logger.LogError($"Exception Type: {ex.GetType().Name}");
                _logger.LogError($"Exception Message: {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }

                ModelState.AddModelError("", $"حدث خطأ أثناء حفظ الوصفة: {ex.Message}");

                if (viewModel != null)
                {
                    await ReloadDropdownData(viewModel);
                    return View(viewModel);
                }

                return Content($"Critical Error: {ex.Message}");
            }
        }

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

        // للاختبار السريع
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
