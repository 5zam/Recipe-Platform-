using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipePlatform.Models.ApplicationModels;

namespace RecipePlatform.DAL.Configuration
{
    public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
    {
        public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
        {
            //throw new NotImplementedException();
            builder.HasOne(ri => ri.Recipe)
               .WithMany(r => r.Ingredients)
               .HasForeignKey(ri => ri.RecipeId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
