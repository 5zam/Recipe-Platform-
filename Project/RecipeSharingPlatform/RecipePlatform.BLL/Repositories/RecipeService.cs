using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipePlatform.BLL.DTOs;
using RecipePlatform.BLL.Interfaces;
using RecipePlatform.DAL.Context;
using RecipePlatform.Models.ApplicationModels;



namespace RecipePlatform.BLL.Repositories
{
    public class RecipeService : GenericRepository<Recipe>, IRecipeService
    {
        private readonly ApplicationDbContext _context;

        public RecipeService(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Recipe>> GetRecipesByUserId(string userId)
        {
            return await _context.Recipes
                         .Include(r => r.Ingredients)
                         .Include(r => r.Instructions)
                         .Include(r => r.Category)
                         .Include(r => r.Author)
                         .Include(r => r.Ratings)
                         .Where(r => r.AuthorId == userId)
                         .OrderByDescending(r => r.CreatedDate)
                         .ToListAsync();
        }

        public async Task<List<Recipe>> GetLatestRecipes(int count)
        {
            return await _context.Recipes
                         .Include(r => r.Category)
                         .Include(r => r.Author)
                         .Include(r => r.Ratings)
                         .Where(r => r.Author.IsActive == true) 
                         .OrderByDescending(r => r.CreatedDate)
                         .Take(count)
                         .ToListAsync();
        }

        public async Task<List<Recipe>> GetTopRatedRecipes(int count)
        {
            var recipesWithRatings = await _context.Recipes
                         .Include(r => r.Category)
                         .Include(r => r.Author)
                         .Include(r => r.Ratings)
                         .Where(r => r.Ratings.Any() && r.Author.IsActive == true)
                         .ToListAsync();

            return recipesWithRatings
                         .OrderByDescending(r => r.Ratings.Any() ? r.Ratings.Average(rating => rating.RateValue) : 0)
                         .Take(count)
                         .ToList();
        }

        public async Task<SearchResultsDto> SearchRecipes(string searchTerm, int page = 1, int pageSize = 12)
        {
            var query = _context.Recipes
                        .Include(r => r.Category)
                        .Include(r => r.Author)
                        .Include(r => r.Ratings)
                        .Include(r => r.Ingredients)
                        .Where(r => r.Author.IsActive == true) 
                        .Where(r =>
                            r.Title.Contains(searchTerm) ||
                            r.Description.Contains(searchTerm) ||
                            r.Ingredients.Any(i => i.Name.Contains(searchTerm))
                        );

            var totalRecipes = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecipes / pageSize);

            var recipes = await query
                          .OrderByDescending(r => r.CreatedDate)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();

            return new SearchResultsDto
            {
                Recipes = recipes,
                SearchQuery = searchTerm,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalRecipes = totalRecipes
            };
        }

        public async Task<SearchResultsDto> GetAllRecipesWithDetails(int page = 1, int pageSize = 12)
        {
            var query = _context.Recipes
                        .Include(r => r.Category)
                        .Include(r => r.Author)
                        .Include(r => r.Ratings)
                        .Where(r => r.Author.IsActive == true); 

            var totalRecipes = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecipes / pageSize);

            var recipes = await query
                          .OrderByDescending(r => r.CreatedDate)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();

            return new SearchResultsDto
            {
                Recipes = recipes,
                SearchQuery = "",
                CurrentPage = page,
                TotalPages = totalPages,
                TotalRecipes = totalRecipes
            };
        }

        public async Task<double> GetAverageRating(int recipeId)
        {
            var ratings = await _context.Rates
                          .Where(r => r.RecipeId == recipeId)
                          .ToListAsync();

            return ratings.Any() ? Math.Round(ratings.Average(r => r.RateValue), 1) : 0;
        }

        public async Task<Recipe> GetRecipeWithRatingsAsync(int id)
        {
            return await _context.Recipes
                         .Include(r => r.Ingredients)
                         .Include(r => r.Instructions)
                         .Include(r => r.Category)
                         .Include(r => r.Author)
                         .Include(r => r.Ratings)
                         .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<int> GetTotalRatingsCount(int recipeId)
        {
            return await _context.Rates
                         .CountAsync(r => r.RecipeId == recipeId);
        }

        public async Task<Rating> GetUserRating(int recipeId, string userId)
        {
            return await _context.Rates
                         .FirstOrDefaultAsync(r => r.RecipeId == recipeId && r.UserId == userId);
        }

        public async Task<bool> HasUserRated(int recipeId, string userId)
        {
            return await _context.Rates
                         .AnyAsync(r => r.RecipeId == recipeId && r.UserId == userId);
        }

        
        public async Task<List<Recipe>> GetPublicRecipesByUserId(string userId)
        {
            return await _context.Recipes
                         .Include(r => r.Ingredients)
                         .Include(r => r.Instructions)
                         .Include(r => r.Category)
                         .Include(r => r.Author)
                         .Include(r => r.Ratings)
                         .Where(r => r.AuthorId == userId && r.Author.IsActive == true) 
                         .OrderByDescending(r => r.CreatedDate)
                         .ToListAsync();
        }
    }
}