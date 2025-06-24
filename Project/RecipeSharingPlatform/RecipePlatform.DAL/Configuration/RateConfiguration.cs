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

            builder.HasOne(rt => rt.User)
                  .WithMany(u => u.Ratings)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Restrict); // delete user → do not delete ratings
        }
    }
}
