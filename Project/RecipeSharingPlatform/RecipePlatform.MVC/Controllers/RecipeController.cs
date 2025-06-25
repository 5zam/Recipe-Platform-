using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RecipePlatform.BLL.Interfaces;
using RecipePlatform.DAL.Context;
using RecipePlatform.Models.ApplicationModels;
using RecipePlatform.Models.ApplicationModels.Enums;
using RecipePlatform.Models.UserModels;
using RecipePlatform.MVC.Models.ViewModels;

namespace RecipePlatform.MVC.Controllers
{
    [Authorize]
    public class RecipeController : Controller
    {
        private readonly IGenericRepository<Recipe> _recipeRepo;
        private readonly IGenericRepository<Category> _catRepo;
        private readonly IRecipeService _recipeService;
        private readonly UserManager<ApplicationUser> _userManager;
        

        public RecipeController(IGenericRepository<Recipe> recipeRepo,
                                UserManager<ApplicationUser> userManager,
                                IRecipeService recipeService,
                                IGenericRepository<Category> catRepo)
        {
            _recipeRepo = recipeRepo;
            _userManager = userManager;
            _recipeService = recipeService;
            _catRepo = catRepo;
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

        // GET: Recipe/Add
        public async Task<IActionResult> Add()
        {
            var categories = await _catRepo.GetAllAsync();

            var viewModel = new AddRecipeVM
            {
                Recipe = new Recipe(),
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }),

                Difficulties = Enum.GetValues(typeof(DifficultyLevel))
                                   .Cast<DifficultyLevel>()
                                   .Select(d => new SelectListItem
                                   {
                                       Value = d.ToString(),
                                       Text = d.ToString()
                                   })
            };

            return View(viewModel);
        }

        //to see data
        private async Task PopulateDropdownLists(AddRecipeVM viewModel)
        {
            var categories = await _catRepo.GetAllAsync();
            viewModel.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });

            viewModel.Difficulties = Enum.GetValues(typeof(DifficultyLevel))
                .Cast<DifficultyLevel>()
                .Select(d => new SelectListItem
                {
                    Value = d.ToString(),
                    Text = d.ToString()
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddRecipeVM viewModel)
        {
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine("🔍 DEBUGGING RECIPE SUBMISSION");
            Console.WriteLine("=".PadRight(60, '='));

            // Print received data
            Console.WriteLine("📥 RECEIVED DATA:");
            Console.WriteLine($"Title: '{viewModel.Recipe.Title}'");
            Console.WriteLine($"Description: '{viewModel.Recipe.Description}'");
            Console.WriteLine($"Ingredients: '{viewModel.Recipe.Ingredients}'");
            Console.WriteLine($"Instructions: '{viewModel.Recipe.Instructions}'");
            Console.WriteLine($"PrepTimeMinutes: {viewModel.Recipe.PrepTimeMinutes}");
            Console.WriteLine($"CookTimeMinutes: {viewModel.Recipe.CookTimeMinutes}");
            Console.WriteLine($"Servings: {viewModel.Recipe.Servings}");
            Console.WriteLine($"Difficulty: '{viewModel.Recipe.Difficulty}'");
            Console.WriteLine($"CategoryId: {viewModel.Recipe.CategoryId}");

            try
            {
                var jsonData = System.Text.Json.JsonSerializer.Serialize(viewModel.Recipe, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                Console.WriteLine("📄 JSON FORMAT:");
                Console.WriteLine(jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ JSON Serialization Error: {ex.Message}");
            }

            Console.WriteLine("🔍 MODEL STATE VALIDATION:");
            Console.WriteLine($"IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ VALIDATION ERRORS:");
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"Field: {error.Key}");
                        foreach (var err in error.Value.Errors)
                        {
                            Console.WriteLine($"  Error: {err.ErrorMessage}");
                        }
                    }
                }

                await PopulateDropdownLists(viewModel);

                TempData["ErrorMessage"] = "ModelState is not valid. Please check your inputs.";
                TempData["ErrorDetails"] = string.Join(" | ",
                    ModelState.SelectMany(ms => ms.Value.Errors.Select(e => e.ErrorMessage)));

                return View(viewModel);
            }

            try
            {
                Console.WriteLine("✅ VALIDATION PASSED - SAVING TO DATABASE");

                var userId = _userManager.GetUserId(User);
                Console.WriteLine($"👤 User ID: {userId}");

                viewModel.Recipe.AuthorId = userId;
                viewModel.Recipe.CreatedDate = DateTime.Now;

                Console.WriteLine("💾 FINAL RECIPE OBJECT BEFORE SAVE:");
                var finalJsonData = System.Text.Json.JsonSerializer.Serialize(viewModel.Recipe, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                Console.WriteLine(finalJsonData);

                await _recipeRepo.AddAsync(viewModel.Recipe);

                Console.WriteLine("✅ RECIPE SAVED SUCCESSFULLY!");
                Console.WriteLine("=".PadRight(60, '='));

                TempData["SuccessMessage"] = "Recipe added successfully!";
                return RedirectToAction("MyRecipes");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ DATABASE SAVE ERROR:");
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                Console.WriteLine("=".PadRight(60, '='));

                TempData["ErrorMessage"] = "An error occurred while saving the recipe: " + ex.Message;

                await PopulateDropdownLists(viewModel);
                return View(viewModel);
            }
        }



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Add(AddRecipeVM viewModel)
        //{
        //    if (!ModelState.IsValid)
        //    {

        //        TempData["ErrorMessage"] = "ModelState is not valid. Please check your inputs.";


        //        var allErrors = ModelState.Values
        //                                   .SelectMany(v => v.Errors)
        //                                   .Select(e => e.ErrorMessage)
        //                                   .ToList();

        //        TempData["ErrorDetails"] = string.Join(" | ", allErrors);

        //        // display all categories
        //        viewModel.Categories = (await _catRepo.GetAllAsync())
        //                                .Select(c => new SelectListItem
        //                                {
        //                                    Value = c.Id.ToString(),
        //                                    Text = c.Name
        //                                });

        //        viewModel.Difficulties = Enum.GetValues(typeof(DifficultyLevel))
        //                                     .Cast<DifficultyLevel>()
        //                                     .Select(d => new SelectListItem
        //                                     {
        //                                         Value = d.ToString(),
        //                                         Text = d.ToString()
        //                                     });


        //        Console.WriteLine("CategoryId from form: " + viewModel.Recipe.CategoryId);

        //        return View(viewModel);
        //    }

        //    try
        //    {
        //        var userId = _userManager.GetUserId(User);
        //        viewModel.Recipe.AuthorId = userId;
        //        viewModel.Recipe.CreatedDate = DateTime.Now;


        //        var recipeJson = System.Text.Json.JsonSerializer.Serialize(viewModel.Recipe);
        //        Console.WriteLine("Recipe JSON: " + recipeJson);

        //        await _recipeRepo.AddAsync(viewModel.Recipe);

        //        TempData["SuccessMessage"] = "added successfully!";
        //        return RedirectToAction("MyRecipes");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "cannot add: " + ex.Message;
        //        return RedirectToAction("MyRecipes");
        //    }

        //}


    }
}
