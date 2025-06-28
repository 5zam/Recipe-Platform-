using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.BLL.DTOs
{
    public class SearchResultsDto
    {
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();
        public string SearchQuery { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalRecipes { get; set; } = 0;
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
