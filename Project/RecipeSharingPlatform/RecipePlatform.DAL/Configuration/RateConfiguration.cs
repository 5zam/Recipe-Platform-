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
    public class RateConfiguration : IEntityTypeConfiguration<Rating>
    {
        public void Configure(EntityTypeBuilder<Rating> builder)
        {
            //throw new NotImplementedException();

            builder.HasKey(r => r.Id);

            builder.Property(r => r.RateValue)
                   .IsRequired()
                   .HasColumnType("decimal(2,1)"); // مثلاً 4.5

            builder.Property(r => r.UserId)
                   .IsRequired();

            builder.Property(r => r.RecipeId)
                   .IsRequired();

            // User relationship
            builder.HasOne(r => r.User)
                   .WithMany(u => u.Ratings)
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Recipe relationship
            builder.HasOne(r => r.Recipe)
                   .WithMany(recipe => recipe.Ratings)
                   .HasForeignKey(r => r.RecipeId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: one rating per user per recipe
            builder.HasIndex(r => new { r.UserId, r.RecipeId })
                   .IsUnique();


            //builder.HasOne(rt => rt.User)
            //      .WithMany(u => u.Ratings)
            //      .HasForeignKey(rt => rt.UserId)
            //      .OnDelete(DeleteBehavior.Restrict); // delete user → do not delete ratings
        }
    }
}
