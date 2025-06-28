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
    public class RecipeInstructionConfiguration : IEntityTypeConfiguration<RecipeInstruction>
    {
        public void Configure(EntityTypeBuilder<RecipeInstruction> builder)
        {
            //throw new NotImplementedException();

            builder.HasKey(ri => ri.Id);

            builder.Property(ri => ri.Description)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(ri => ri.StepNumber)
                   .IsRequired();

            // Recipe relationship
            builder.HasOne(ri => ri.Recipe)
                   .WithMany(r => r.Instructions)
                   .HasForeignKey(ri => ri.RecipeId)
                   .OnDelete(DeleteBehavior.Cascade);


            //builder.HasOne(ri => ri.Recipe)
            //    .WithMany(r => r.Instructions)
            //    .HasForeignKey(ri => ri.RecipeId)
            //    .OnDelete(DeleteBehavior.Cascade);


            //builder.HasIndex(ri => new { ri.RecipeId, ri.StepNumber })
            //    .IsUnique();
        }

    }
}
