using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipePlatform.BLL.Interfaces;
using RecipePlatform.BLL.Repositories;
using RecipePlatform.DAL.Context;
using RecipePlatform.Models.ApplicationModels;
using RecipePlatform.Models.UserModels;

namespace RecipePlatform.MVC.Controllers
{
    [Authorize]
    public class RatingController : Controller
    {
        private readonly IRatingService _ratingService;
        private readonly IRecipeService _recipeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RatingController> _logger;

        public RatingController(
            IRatingService ratingService,
            IRecipeService recipeService,
            UserManager<ApplicationUser> userManager,
            ILogger<RatingController> logger)
        {
            _ratingService = ratingService;
            _recipeService = recipeService;
            _userManager = userManager;
            _logger = logger;
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRating(int recipeId, int rateValue)
        {
            try
            {
                _logger.LogInformation($"⭐ AddRating called for Recipe {recipeId} with value {rateValue}");

                // Validate input
                if (rateValue < 1 || rateValue > 5)
                {
                    _logger.LogWarning($"❌ Invalid rating value: {rateValue}");
                    TempData["ErrorMessage"] = "Rating must be between 1 and 5 stars";
                    return RedirectToAction("Details", "Recipe", new { id = recipeId });
                }

                // Check if recipe exists
                var recipe = await _recipeService.GetByIdAsync(recipeId);
                if (recipe == null)
                {
                    _logger.LogWarning($"❌ Recipe {recipeId} not found");
                    TempData["ErrorMessage"] = "Recipe not found";
                    return RedirectToAction("Index", "Home");
                }

                // Get current user
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("❌ User not authenticated");
                    return RedirectToAction("Login", "Account");
                }

                // Check if user is trying to rate their own recipe
                if (recipe.AuthorId == userId)
                {
                    _logger.LogWarning($"❌ User {userId} trying to rate their own recipe {recipeId}");
                    TempData["ErrorMessage"] = "You cannot rate your own recipe";
                    return RedirectToAction("Details", "Recipe", new { id = recipeId });
                }

                // Check if this was an update
                var wasUpdate = await _ratingService.HasUserRatedAsync(recipeId, userId);

                // Add or update rating
                var rating = await _ratingService.AddOrUpdateRatingAsync(recipeId, userId, rateValue);

                if (wasUpdate)
                {
                    _logger.LogInformation($"✅ Rating updated successfully for Recipe {recipeId}");
                    TempData["SuccessMessage"] = $"Your rating has been updated to {rateValue} stars! ⭐";
                }
                else
                {
                    _logger.LogInformation($"✅ New rating added successfully for Recipe {recipeId}");
                    TempData["SuccessMessage"] = $"Thank you for rating this recipe {rateValue} stars! ⭐";
                }

                return RedirectToAction("Details", "Recipe", new { id = recipeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error processing rating for recipe {recipeId}");
                TempData["ErrorMessage"] = "An error occurred while saving your rating. Please try again.";
                return RedirectToAction("Details", "Recipe", new { id = recipeId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRating(int recipeId)
        {
            try
            {
                _logger.LogInformation($"🗑️ RemoveRating called for Recipe {recipeId}");

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("❌ User not authenticated");

                    // Return JSON for AJAX requests
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { Success = false, Message = "User not authenticated" });
                    }

                    return RedirectToAction("Login", "Account");
                }

                // Check if user has rated this recipe
                var userRating = await _ratingService.GetUserRatingAsync(recipeId, userId);
                if (userRating == null)
                {
                    _logger.LogWarning($"❌ No rating found for User {userId} on Recipe {recipeId}");

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { Success = false, Message = "You haven't rated this recipe yet" });
                    }

                    TempData["ErrorMessage"] = "You haven't rated this recipe yet";
                    return RedirectToAction("Details", "Recipe", new { id = recipeId });
                }

                // Remove the rating
                var success = await _ratingService.RemoveRatingAsync(recipeId, userId);

                if (success)
                {
                    _logger.LogInformation($"✅ Rating removed successfully for Recipe {recipeId}");

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { Success = true, Message = "Your rating has been removed" });
                    }

                    TempData["SuccessMessage"] = "Your rating has been removed";
                }
                else
                {
                    _logger.LogWarning($"❌ Failed to remove rating for Recipe {recipeId}");

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { Success = false, Message = "Failed to remove rating" });
                    }

                    TempData["ErrorMessage"] = "Failed to remove rating";
                }

                return RedirectToAction("Details", "Recipe", new { id = recipeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error removing rating for recipe {recipeId}");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { Success = false, Message = "An error occurred while removing your rating" });
                }

                TempData["ErrorMessage"] = "An error occurred while removing your rating. Please try again.";
                return RedirectToAction("Details", "Recipe", new { id = recipeId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRatingStats(int recipeId)
        {
            try
            {
                _logger.LogInformation($"📊 GetRatingStats called for Recipe {recipeId}");

                var averageRating = await _ratingService.GetAverageRatingAsync(recipeId);
                var totalRatings = await _ratingService.GetTotalRatingsAsync(recipeId);
                var ratingDistribution = await _ratingService.GetRatingDistributionAsync(recipeId);

                var result = new
                {
                    Success = true,
                    AverageRating = averageRating,
                    TotalRatings = totalRatings,
                    RatingDistribution = ratingDistribution
                };

                _logger.LogInformation($"📊 Rating stats for Recipe {recipeId}: Avg={averageRating:F1}, Total={totalRatings}");
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting rating stats for recipe {recipeId}");
                return Json(new { Success = false, Message = "Error loading rating statistics" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserRating(int recipeId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { Success = false, Message = "User not authenticated" });
                }

                var userRating = await _ratingService.GetUserRatingAsync(recipeId, userId);

                var result = new
                {
                    Success = true,
                    HasRated = userRating != null,
                    RatingValue = userRating?.RateValue ?? 0
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting user rating for recipe {recipeId}");
                return Json(new { Success = false, Message = "Error loading user rating" });
            }
        }

        // AJAX endpoint for real-time rating updates (for backwards compatibility)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRatingAjax(int recipeId, int rateValue)
        {
            try
            {
                _logger.LogInformation($"⭐ AJAX UpdateRating called for Recipe {recipeId} with value {rateValue}");

                // Validate input
                if (rateValue < 1 || rateValue > 5)
                {
                    return Json(new { Success = false, Message = "Rating must be between 1 and 5 stars" });
                }

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { Success = false, Message = "User not authenticated" });
                }

                // Check if recipe exists
                var recipe = await _recipeService.GetByIdAsync(recipeId);
                if (recipe == null)
                {
                    return Json(new { Success = false, Message = "Recipe not found" });
                }

                // Check if user is trying to rate their own recipe
                if (recipe.AuthorId == userId)
                {
                    return Json(new { Success = false, Message = "You cannot rate your own recipe" });
                }

                // Check if user has already rated
                var wasUpdate = await _ratingService.HasUserRatedAsync(recipeId, userId);

                // Add or update rating
                var rating = await _ratingService.AddOrUpdateRatingAsync(recipeId, userId, rateValue);

                // Get updated statistics
                var newAverageRating = await _ratingService.GetAverageRatingAsync(recipeId);
                var newTotalRatings = await _ratingService.GetTotalRatingsAsync(recipeId);

                var result = new
                {
                    Success = true,
                    Message = wasUpdate ? "Rating updated successfully!" : "Rating added successfully!",
                    UserRating = rateValue,
                    AverageRating = newAverageRating,
                    TotalRatings = newTotalRatings,
                    WasUpdate = wasUpdate
                };

                _logger.LogInformation($"✅ AJAX rating {(wasUpdate ? "updated" : "added")} successfully");
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ AJAX Error processing rating for recipe {recipeId}");
                return Json(new { Success = false, Message = "An error occurred while saving your rating" });
            }
        }
    }
}


