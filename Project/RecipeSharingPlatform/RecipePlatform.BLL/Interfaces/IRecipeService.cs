using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.BLL.Interfaces
{
    public interface IRecipeService : IGenericRepository<Recipe>
    {
        Task<List<Recipe>> GetRecipesByUserId(string userId);
    }
}
