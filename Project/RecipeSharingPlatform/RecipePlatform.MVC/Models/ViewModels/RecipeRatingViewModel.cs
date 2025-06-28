using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.MVC.Models.ViewModels
{
    public class RecipeRatingViewModel
    {
        public Recipe Recipe { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int? UserRating { get; set; }
        public bool HasUserRated { get; set; }
        public bool IsUserLoggedIn { get; set; }
    }
}
