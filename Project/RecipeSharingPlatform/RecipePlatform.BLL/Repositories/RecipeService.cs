using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecipePlatform.BLL.Interfaces;
using RecipePlatform.DAL.Context;
using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.BLL.Repositories
{
    public class RecipeService : GenericRepository<Recipe>, IRecipeService
    {
        //private readonly IGenericRepository<Recipe> _recipeRepo;
        private readonly ApplicationDbContext _context;

        public RecipeService(ApplicationDbContext context) : base(context)
        {
            _context= context;
        }

        public async Task<List<Recipe>> GetRecipesByUserId(string userId)
        {
            return await _context.Recipes
                         .Include(r => r.Ingredients)
                         .Include(r => r.Instructions)
                         .Where(r => r.AuthorId == userId)
                         .ToListAsync();
        }
    }
}
