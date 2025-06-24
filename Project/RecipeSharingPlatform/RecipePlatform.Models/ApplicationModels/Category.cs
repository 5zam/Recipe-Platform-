using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.Models.ApplicationModels
{
    public class Category: BaseEntity
    {
        public string? Name { get; set; }
        public IEnumerable<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}
