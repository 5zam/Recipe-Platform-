namespace RecipePlatform.MVC.Models.ViewModels
{
    public class UserBrowseListViewModel
    {
        public List<UserBrowseViewModel> Users { get; set; } = new List<UserBrowseViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalUsers { get; set; }
    }
}
