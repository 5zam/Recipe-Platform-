using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.MVC.Models.ViewModels
{
    public class AddRecipeVM
    {
        public Recipe Recipe { get; set; }

        [BindNever]
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem>? Difficulties { get; set; }

        public List<string> IngredientsList { get; set; } = new();
        public List<string> InstructionsList { get; set; } = new();
    }
}
