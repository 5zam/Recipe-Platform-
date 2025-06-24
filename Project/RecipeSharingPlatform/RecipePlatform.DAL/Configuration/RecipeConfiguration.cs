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
    public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipe> builder)
        {
            //throw new NotImplementedException();


            // Author (User) relationship
            builder.HasOne(r => r.Author)
                   .WithMany(u => u.Recipes)
                   .HasForeignKey(r => r.AuthorId)
                   .OnDelete(DeleteBehavior.Cascade);      // delete user → delete recipes

            // Ratings
            builder.HasMany(r => r.Ratings)
                   .WithOne(rt => rt.Recipe)
                   .HasForeignKey(rt => rt.RecipeId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
