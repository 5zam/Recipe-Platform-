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
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            //throw new NotImplementedException();

            builder.HasMany(c => c.Recipes)
                  .WithOne(r => r.Category)
                  .HasForeignKey(r => r.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
