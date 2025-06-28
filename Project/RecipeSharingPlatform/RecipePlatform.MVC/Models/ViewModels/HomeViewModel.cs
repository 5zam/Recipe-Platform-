using RecipePlatform.Models.ApplicationModels;
using RecipePlatform.Models.UserModels;

namespace RecipePlatform.MVC.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Recipe> LatestRecipes { get; set; } = new List<Recipe>();
        public List<Recipe> TopRatedRecipes { get; set; } = new List<Recipe>();
    }

    public class SearchResultsViewModel
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