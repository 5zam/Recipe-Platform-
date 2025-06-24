using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.MVC.Models.ViewModels
{
    public class MyRecipesVM
    {
            public string UserName { get; set; }
            public List<Recipe> Recipes { get; set; }
        

    }
}
