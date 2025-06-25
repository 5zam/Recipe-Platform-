using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.Models.ApplicationModels
{
    public class RecipeIngredient : BaseEntity
    {
        [Required]
        public string Name { get; set; }

        public string? Quantity { get; set; } 

        public int Order { get; set; } 

        // Foreign Key
        [ForeignKey("RecipeId")]
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}
