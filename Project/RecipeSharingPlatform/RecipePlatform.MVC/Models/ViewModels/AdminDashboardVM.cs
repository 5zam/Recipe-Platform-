using RecipePlatform.Models.ApplicationModels;
using RecipePlatform.Models.UserModels;

namespace RecipePlatform.MVC.Models.ViewModels
{
    public class AdminDashboardVM
    {
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();
        public List<Category> Categories { get; set; } = new List<Category>();

        // البطاقات اللي فوق 
        public int TotalUsers { get; set; }
        public int SuspendedUsers { get; set; }
        public int TotalRecipes { get; set; }
    }

    }

