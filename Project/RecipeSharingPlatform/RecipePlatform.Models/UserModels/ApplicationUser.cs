using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RecipePlatform.Models.ApplicationModels;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace RecipePlatform.Models.UserModels
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Recipe>? Recipes { get; set; }
        public ICollection<Rating>? Ratings { get; set; }
    }
}
