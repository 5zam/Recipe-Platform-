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

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(r => r.Description)
                   .HasMaxLength(1000);

            builder.Property(r => r.AuthorId)
                   .IsRequired();

            // Author (User) relationship
            builder.HasOne(r => r.Author)
                   .WithMany(u => u.Recipes)
                   .HasForeignKey(r => r.AuthorId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Category relationship
            builder.HasOne(r => r.Category)
                   .WithMany()
                   .HasForeignKey(r => r.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Ingredients relationship
            builder.HasMany(r => r.Ingredients)
                   .WithOne(i => i.Recipe)
                   .HasForeignKey(i => i.RecipeId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Instructions relationship
            builder.HasMany(r => r.Instructions)
                   .WithOne(i => i.Recipe)
                   .HasForeignKey(i => i.RecipeId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Ratings relationship
            builder.HasMany(r => r.Ratings)
                   .WithOne(rt => rt.Recipe)
                   .HasForeignKey(rt => rt.RecipeId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Author (User) relationship
            //builder.HasOne(r => r.Author)
            //       .WithMany(u => u.Recipes)
            //       .HasForeignKey(r => r.AuthorId)
            //       .OnDelete(DeleteBehavior.Cascade);      // delete user → delete recipes

            //// Ratings
            //builder.HasMany(r => r.Ratings)
            //       .WithOne(rt => rt.Recipe)
            //       .HasForeignKey(rt => rt.RecipeId)
            //       .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
