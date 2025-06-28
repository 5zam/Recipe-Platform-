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

            builder.HasKey(ri => ri.Id);

            builder.Property(ri => ri.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(ri => ri.Order)
                   .IsRequired();

            // Recipe relationship
            builder.HasOne(ri => ri.Recipe)
                   .WithMany(r => r.Ingredients)
                   .HasForeignKey(ri => ri.RecipeId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

            //builder.HasOne(ri => ri.Recipe)
            //   .WithMany(r => r.Ingredients)
            //   .HasForeignKey(ri => ri.RecipeId)
            //   .OnDelete(DeleteBehavior.Cascade);
