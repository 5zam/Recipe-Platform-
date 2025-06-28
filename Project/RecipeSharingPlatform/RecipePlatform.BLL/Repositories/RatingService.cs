using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RecipePlatform.BLL.Interfaces;
using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.BLL.Repositories
{
    public class RatingService : IRatingService
    {
        private readonly IGenericRepository<Rating> _ratingRepo;
        private readonly ILogger<RatingService> _logger;

        public RatingService(
            IGenericRepository<Rating> ratingRepo,
            ILogger<RatingService> logger)
        {
            _ratingRepo = ratingRepo;
            _logger = logger;
        }

        public async Task<Rating> AddOrUpdateRatingAsync(int recipeId, string userId, int rateValue)
        {
            try
            {
                _logger.LogInformation($"⭐ Adding/Updating rating for Recipe {recipeId} by User {userId}");

                var existingRating = await GetUserRatingAsync(recipeId, userId);

                if (existingRating != null)
                {
                    _logger.LogInformation($"📝 Updating existing rating from {existingRating.RateValue} to {rateValue}");
                    existingRating.RateValue = rateValue;
                    existingRating.CreatedAt = DateTime.Now;
                    await _ratingRepo.UpdateAsync(existingRating);
                    return existingRating;
                }
                else
                {
                    _logger.LogInformation($"✨ Creating new rating with value {rateValue}");
                    var newRating = new Rating
                    {
                        RecipeId = recipeId,
                        UserId = userId,
                        RateValue = rateValue,
                        CreatedAt = DateTime.Now
                    };
                    await _ratingRepo.AddAsync(newRating);
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
                var ratings = await _ratingRepo.GetAllAsync();
                var recipeRatings = ratings.Where(r => r.RecipeId == recipeId).ToList();

                if (!recipeRatings.Any())
                    return 0;

                return Math.Round(recipeRatings.Average(r => r.RateValue), 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting average rating for Recipe {recipeId}");
                return 0;
            }
        }

        public async Task<int> GetTotalRatingsAsync(int recipeId)
        {
            try
            {
                var ratings = await _ratingRepo.GetAllAsync();
                return ratings.Count(r => r.RecipeId == recipeId);
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
                var ratings = await _ratingRepo.GetAllAsync();
                return ratings.FirstOrDefault(r => r.RecipeId == recipeId && r.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting user rating for Recipe {recipeId}, User {userId}");
                return null;
            }
        }

        public async Task<bool> HasUserRatedAsync(int recipeId, string userId)
        {
            var userRating = await GetUserRatingAsync(recipeId, userId);
            return userRating != null;
        }

        public async Task<List<Rating>> GetRecipeRatingsAsync(int recipeId)
        {
            try
            {
                var ratings = await _ratingRepo.GetAllAsync();
                return ratings.Where(r => r.RecipeId == recipeId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error getting ratings for Recipe {recipeId}");
                return new List<Rating>();
            }
        }

        public Task<bool> RemoveRatingAsync(int recipeId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<int, int>> GetRatingDistributionAsync(int recipeId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CanUserRateRecipeAsync(int recipeId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Rating>> GetTopRatedRecipeRatingsAsync(int count = 10)
        {
            throw new NotImplementedException();
        }
    }
}