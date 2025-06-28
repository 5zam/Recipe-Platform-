using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipePlatform.BLL.Interfaces;
using RecipePlatform.DAL.Context;
using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.BLL.Repositories
{
    public class EnhancedRatingService : IRatingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnhancedRatingService> _logger;

        public EnhancedRatingService(
            ApplicationDbContext context,
            ILogger<EnhancedRatingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Rating> AddOrUpdateRatingAsync(int recipeId, string userId, int rateValue)
        {
            try
            {
                _logger.LogInformation($"⭐ Adding/Updating rating for Recipe {recipeId} by User {userId} with value {rateValue}");

                // Validate input
                if (rateValue < 1 || rateValue > 5)
                {
                    throw new ArgumentException("Rating value must be between 1 and 5", nameof(rateValue));
                }

                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
                }

                if (recipeId <= 0)
                {
                    throw new ArgumentException("Recipe ID must be positive", nameof(recipeId));
                }

                // Check if rating already exists
                var existingRating = await _context.Rates
                    .FirstOrDefaultAsync(r => r.RecipeId == recipeId && r.UserId == userId);

                if (existingRating != null)
                {
                    // Update existing rating
                    _logger.LogInformation($"📝 Updating existing rating from {existingRating.RateValue} to {rateValue}");

                    existingRating.RateValue = rateValue;
                    existingRating.CreatedAt = DateTime.Now;

                    _context.Rates.Update(existingRating);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"✅ Rating updated successfully");
                    return existingRating;
                }
                else
                {
                    // Create new rating
                    _logger.LogInformation($"✨ Creating new rating with value {rateValue}");

                    var newRating = new Rating
                    {
                        RecipeId = recipeId,
                        UserId = userId,
                        RateValue = rateValue,
                        CreatedAt = DateTime.Now
                    };

                    _context.Rates.Add(newRating);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"✅ New rating created successfully with ID {newRating.Id}");
                    return newRating;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error adding/updating rating for Recipe {recipeId}");
                throw;
            }
        }

        public async Task<double> GetAverageRatingAsync(int recipeId)
        {
            try
            {
                _logger.LogDebug($"📊 Calculating average rating for Recipe {recipeId}");

                var ratings = await _context.Rates
                    .Where(r => r.RecipeId == recipeId)
                    .Select(r => r.RateValue)
                    .ToListAsync();

                if (!ratings.Any())
                {
                    _logger.LogDebug($"📊 No ratings found for Recipe {recipeId}");
                    return 0.0;
                }

                var average = Math.Round(ratings.Average(), 1);
                _logger.LogDebug($"📊 Average rating for Recipe {recipeId}: {average} (from {ratings.Count} ratings)");

                return average;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error calculating average rating for Recipe {recipeId}");
                return 0.0;
            }
        }

        public async Task<int> GetTotalRatingsAsync(int recipeId)
        {
            try
            {
                var count = await _context.Rates
                    .CountAsync(r => r.RecipeId == recipeId);

                _logger.LogDebug($"📊 Total ratings for Recipe {recipeId}: {count}");
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting total ratings for Recipe {recipeId}");
                return 0;
            }
        }

        public async Task<Rating> GetUserRatingAsync(int recipeId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("❌ UserId is null or empty in GetUserRatingAsync");
                    return null;
                }

                var rating = await _context.Rates
                    .Include(r => r.User)
                    .Include(r => r.Recipe)
                    .FirstOrDefaultAsync(r => r.RecipeId == recipeId && r.UserId == userId);

                if (rating != null)
                {
                    _logger.LogDebug($"👤 Found user rating: {rating.RateValue} stars for Recipe {recipeId} by User {userId}");
                }
                else
                {
                    _logger.LogDebug($"👤 No rating found for Recipe {recipeId} by User {userId}");
                }

                return rating;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting user rating for Recipe {recipeId}, User {userId}");
                return null;
            }
        }

        public async Task<bool> HasUserRatedAsync(int recipeId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                var hasRated = await _context.Rates
                    .AnyAsync(r => r.RecipeId == recipeId && r.UserId == userId);

                _logger.LogDebug($"👤 User {userId} has{(hasRated ? "" : " not")} rated Recipe {recipeId}");
                return hasRated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error checking if user has rated Recipe {recipeId}");
                return false;
            }
        }

        public async Task<List<Rating>> GetRecipeRatingsAsync(int recipeId)
        {
            try
            {
                var ratings = await _context.Rates
                    .Include(r => r.User)
                    .Where(r => r.RecipeId == recipeId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                _logger.LogDebug($"📋 Retrieved {ratings.Count} ratings for Recipe {recipeId}");
                return ratings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting ratings for Recipe {recipeId}");
                return new List<Rating>();
            }
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int recipeId)
        {
            try
            {
                _logger.LogDebug($"📊 Getting rating distribution for Recipe {recipeId}");

                var ratings = await _context.Rates
                    .Where(r => r.RecipeId == recipeId)
                    .GroupBy(r => r.RateValue)
                    .Select(g => new { RateValue = g.Key, Count = g.Count() })
                    .ToListAsync();

                var distribution = new Dictionary<int, int>();

                // Initialize all star counts to 0
                for (int i = 1; i <= 5; i++)
                {
                    distribution[i] = 0;
                }

                // Fill in actual counts
                foreach (var rating in ratings)
                {
                    distribution[rating.RateValue] = rating.Count;
                }

                _logger.LogDebug($"📊 Rating distribution for Recipe {recipeId}: " +
                    $"1⭐:{distribution[1]}, 2⭐:{distribution[2]}, 3⭐:{distribution[3]}, " +
                    $"4⭐:{distribution[4]}, 5⭐:{distribution[5]}");

                return distribution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting rating distribution for Recipe {recipeId}");
                return new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };
            }
        }

        public async Task<List<Rating>> GetTopRatedRecipeRatingsAsync(int count = 10)
        {
            try
            {
                var topRatedRecipes = await _context.Rates
                    .Include(r => r.Recipe)
                    .Include(r => r.User)
                    .GroupBy(r => r.RecipeId)
                    .Select(g => new
                    {
                        RecipeId = g.Key,
                        AverageRating = g.Average(r => r.RateValue),
                        TotalRatings = g.Count()
                    })
                    .Where(x => x.TotalRatings >= 5) // Only recipes with at least 5 ratings
                    .OrderByDescending(x => x.AverageRating)
                    .ThenByDescending(x => x.TotalRatings)
                    .Take(count)
                    .ToListAsync();

                var topRatings = new List<Rating>();
                foreach (var item in topRatedRecipes)
                {
                    var rating = await _context.Rates
                        .Include(r => r.Recipe)
                        .Include(r => r.User)
                        .FirstOrDefaultAsync(r => r.RecipeId == item.RecipeId);

                    if (rating != null)
                    {
                        topRatings.Add(rating);
                    }
                }

                return topRatings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting top rated recipe ratings");
                return new List<Rating>();
            }
        }

        public async Task<bool> RemoveRatingAsync(int recipeId, string userId)
        {
            try
            {
                _logger.LogInformation($"🗑️ Removing rating for Recipe {recipeId} by User {userId}");

                var rating = await _context.Rates
                    .FirstOrDefaultAsync(r => r.RecipeId == recipeId && r.UserId == userId);

                if (rating == null)
                {
                    _logger.LogWarning($"❌ No rating found to remove for Recipe {recipeId} by User {userId}");
                    return false;
                }

                _context.Rates.Remove(rating);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Rating removed successfully for Recipe {recipeId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error removing rating for Recipe {recipeId}");
                return false;
            }
        }

        public async Task<bool> CanUserRateRecipeAsync(int recipeId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                // Check if recipe exists
                var recipe = await _context.Recipes
                    .FirstOrDefaultAsync(r => r.Id == recipeId);

                if (recipe == null)
                {
                    _logger.LogWarning($"❌ Recipe {recipeId} not found");
                    return false;
                }

                // Users cannot rate their own recipes
                if (recipe.AuthorId == userId)
                {
                    _logger.LogDebug($"👤 User {userId} cannot rate their own recipe {recipeId}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error checking if user can rate recipe {recipeId}");
                return false;
            }
        }
    }
}
    