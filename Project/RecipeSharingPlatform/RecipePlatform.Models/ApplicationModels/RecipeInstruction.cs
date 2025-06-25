using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.Models.ApplicationModels
{
    public class RecipeInstruction : BaseEntity
    {
        [Required]
        public string Description { get; set; }

        public int StepNumber { get; set; }

        // Foreign Key
        [ForeignKey("RecipeId")]
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}
