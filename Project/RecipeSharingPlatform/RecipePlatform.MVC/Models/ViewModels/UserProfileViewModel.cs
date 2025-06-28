using RecipePlatform.Models.ApplicationModels;
using RecipePlatform.Models.UserModels;

namespace RecipePlatform.MVC.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();
        public int TotalRecipes { get; set; }
        public DateTime JoinedDate { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatingsReceived { get; set; }
        public bool IsOwnProfile { get; set; }
    }
}
