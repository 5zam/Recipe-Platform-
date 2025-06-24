using RecipePlatform.Models.ApplicationModels;
using RecipePlatform.Models.UserModels;

namespace RecipePlatform.MVC.Models.ViewModels
{
    public class AdminDashboardVM
    {
        public List<ApplicationUser> Users { get; set; }
        public List<Recipe> Recipes { get; set; }
        public List<Category> Categories { get; set; }

        // ➕البطاقات اللي فوق 
        public int TotalUsers { get; set; }
        public int SuspendedUsers { get; set; }
        public int TotalRecipes { get; set; }
    }

    }

