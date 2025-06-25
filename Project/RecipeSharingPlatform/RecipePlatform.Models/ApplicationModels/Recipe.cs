using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlatform.Models.ApplicationModels.Enums;
using RecipePlatform.Models.UserModels;

namespace RecipePlatform.Models.ApplicationModels
{
    public class Recipe : BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? PrepTimeMinutes { get; set; }
        public int? CookTimeMinutes { get; set; }
        public int? Servings { get; set; }
        public DifficultyLevel? Difficulty { get; set; }
        public DateTime? CreatedDate { get; set; }

        // Navigation properties 
        [ForeignKey("AuthorId")]
        public string? AuthorId { get; set; }
        public ApplicationUser? Author { get; set; }

        [ForeignKey("CategoryId")]
        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
        public ICollection<RecipeInstruction> Instructions { get; set; } = new List<RecipeInstruction>();

        public IEnumerable<Rating> Ratings { get; set; }= new List<Rating>();
    }
}
