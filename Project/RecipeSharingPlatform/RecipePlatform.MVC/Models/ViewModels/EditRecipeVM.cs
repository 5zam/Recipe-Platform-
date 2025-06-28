using Microsoft.AspNetCore.Mvc.Rendering;
using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.MVC.Models.ViewModels
{
    public class EditRecipeVM
    {
        public Recipe Recipe { get; set; } = new Recipe();
        public List<string> IngredientsList { get; set; } = new List<string>();
        public List<string> InstructionsList { get; set; } = new List<string>();
        public List<string> QuantitiesList { get; set; } = new List<string>(); 
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Difficulties { get; set; } = new List<SelectListItem>();
    }

}
