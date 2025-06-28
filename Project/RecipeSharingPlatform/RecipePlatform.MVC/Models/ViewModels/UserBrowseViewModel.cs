namespace RecipePlatform.MVC.Models.ViewModels
{
    public class UserBrowseViewModel
    {
        public string UserName { get; set; }
        public DateTime JoinedDate { get; set; }
        public int TotalRecipes { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
    }
}
