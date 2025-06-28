using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlatform.BLL.DTOs;
using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.BLL.Interfaces
{
    public interface IRecipeService : IGenericRepository<Recipe>
    {
        Task<List<Recipe>> GetRecipesByUserId(string userId);
        Task<List<Recipe>> GetLatestRecipes(int count);
        Task<List<Recipe>> GetTopRatedRecipes(int count);
        Task<SearchResultsDto> SearchRecipes(string searchTerm, int page = 1, int pageSize = 12);
        Task<SearchResultsDto> GetAllRecipesWithDetails(int page = 1, int pageSize = 12);
        Task<double> GetAverageRating(int recipeId);
        Task<Recipe> GetRecipeWithRatingsAsync(int id);

       
        Task<int> GetTotalRatingsCount(int recipeId);
        Task<Rating> GetUserRating(int recipeId, string userId);
        Task<bool> HasUserRated(int recipeId, string userId);
    }
}
