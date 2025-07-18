﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlatform.Models.UserModels;

namespace RecipePlatform.Models.ApplicationModels
{
    public class Rating : BaseEntity
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int RateValue { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties 
        [ForeignKey("RecipeId")]
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }


        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
