using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipePlatform.BLL.Interfaces;
using RecipePlatform.Models.UserModels;
using RecipePlatform.MVC.Models.ViewModels;

namespace RecipePlatform.MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRecipeService _recipeService;
        private readonly IRatingService _ratingService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserManager<ApplicationUser> userManager,
            IRecipeService recipeService,
            IRatingService ratingService,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _recipeService = recipeService;
            _ratingService = ratingService;
            _logger = logger;
        }

        /// <summary>
        /// Public user profile - shows user's recipes for browsing (read-only)
        /// Different from RecipeController.MyRecipes (which is for editing)
        /// </summary>
        public async Task<IActionResult> Profile(string username)
        {
            try
            {
                _logger.LogInformation($"👤 Profile requested for username: {username}");

                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("❌ Username is null or empty");
                    return NotFound();
                }

                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning($"❌ User '{username}' not found");
                    return NotFound();
                }

                // Check if user is suspended
                if (!user.IsActive)
                {
                    _logger.LogWarning($"❌ User '{username}' is suspended");
                    TempData["ErrorMessage"] = "This user account has been suspended and is not accessible.";
                    return RedirectToAction("MyRecipes", "Recipe");
                }

                // Get user's recipes
                var userRecipes = await _recipeService.GetRecipesByUserId(user.Id);
                _logger.LogInformation($"📚 Found {userRecipes.Count} recipes for user {username}");

                // Calculate user statistics
                var totalRatings = 0;
                var totalRatingValue = 0.0;
                foreach (var recipe in userRecipes)
                {
                    var recipeRatings = await _ratingService.GetRecipeRatingsAsync(recipe.Id);
                    totalRatings += recipeRatings.Count;
                    totalRatingValue += recipeRatings.Sum(r => r.RateValue);
                }

                var averageUserRating = totalRatings > 0 ? Math.Round(totalRatingValue / totalRatings, 1) : 0.0;

                // Check if current user is viewing their own profile
                var currentUserId = _userManager.GetUserId(User);
                var isOwnProfile = currentUserId == user.Id;

                var viewModel = new UserProfileViewModel
                {
                    User = user,
                    Recipes = userRecipes,
                    TotalRecipes = userRecipes.Count,
                    JoinedDate = user.CreatedAt ?? DateTime.Now,
                    AverageRating = averageUserRating,
                    TotalRatingsReceived = totalRatings,
                    IsOwnProfile = isOwnProfile
                };

                _logger.LogInformation($"✅ Profile loaded for {username} - {viewModel.TotalRecipes} recipes, {averageUserRating:F1} avg rating");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error loading profile for user '{username}'");
                return Content($"Error loading user profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Browse all users (optional feature)
        /// </summary>
        public async Task<IActionResult> Browse(int page = 1, int pageSize = 12)
        {
            try
            {
                _logger.LogInformation($"👥 Browse users called - Page {page}");

                var allUsers = _userManager.Users.Where(u => u.IsActive).ToList();
                var totalUsers = allUsers.Count;
                var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

                var usersForPage = allUsers
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var userProfiles = new List<UserBrowseViewModel>();

                foreach (var user in usersForPage)
                {
                    var userRecipes = await _recipeService.GetRecipesByUserId(user.Id);
                    var recipeCount = userRecipes.Count;

                    // Calculate average rating for this user's recipes
                    var totalRatings = 0;
                    var totalRatingValue = 0.0;
                    foreach (var recipe in userRecipes)
                    {
                        var recipeRatings = await _ratingService.GetRecipeRatingsAsync(recipe.Id);
                        totalRatings += recipeRatings.Count;
                        totalRatingValue += recipeRatings.Sum(r => r.RateValue);
                    }

                    var averageRating = totalRatings > 0 ? Math.Round(totalRatingValue / totalRatings, 1) : 0.0;

                    userProfiles.Add(new UserBrowseViewModel
                    {
                        UserName = user.UserName,
                        JoinedDate = user.CreatedAt ?? DateTime.Now,
                        TotalRecipes = recipeCount,
                        AverageRating = averageRating,
                        TotalRatings = totalRatings
                    });
                }

                var viewModel = new UserBrowseListViewModel
                {
                    Users = userProfiles,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalUsers = totalUsers
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error browsing users");
                return Content($"Error browsing users: {ex.Message}");
            }
        }

        // Test method
        public IActionResult Test()
        {
            return Content("UserController is working! 🎉");
        }
    }
}