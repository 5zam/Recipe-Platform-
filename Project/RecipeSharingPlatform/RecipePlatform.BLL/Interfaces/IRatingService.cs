using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipePlatform.DAL.Context;
using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.BLL.Interfaces
{
    public interface IRatingService 
    {
        // Core rating operations
        Task<Rating> AddOrUpdateRatingAsync(int recipeId, string userId, int rateValue);
        Task<bool> RemoveRatingAsync(int recipeId, string userId);

        // Rating statistics
        Task<double> GetAverageRatingAsync(int recipeId);
        Task<int> GetTotalRatingsAsync(int recipeId);
        Task<Dictionary<int, int>> GetRatingDistributionAsync(int recipeId);

        // User-specific rating operations
        Task<Rating> GetUserRatingAsync(int recipeId, string userId);
        Task<bool> HasUserRatedAsync(int recipeId, string userId);
        Task<bool> CanUserRateRecipeAsync(int recipeId, string userId);

        // Retrieve ratings
        Task<List<Rating>> GetRecipeRatingsAsync(int recipeId);
        Task<List<Rating>> GetTopRatedRecipeRatingsAsync(int count = 10);
    }

    
}

