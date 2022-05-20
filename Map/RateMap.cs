using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Constant;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Map
{
    public class RateMap : IEntityTypeConfiguration<Rate>
    {
        public void Configure(EntityTypeBuilder<Rate> builder)
        {
            SetUpRate(builder);
        }
        public void SetUpRate(EntityTypeBuilder<Rate> builder)
        {
            var ten = new Rate
            {
                Id = Defaults.RateTenId,
                Amount = 10,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
            };
            var hundred = new Rate
            {
                Id = Defaults.RateHundredId,
                Amount = 100,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
            };
            var twoHundred = new Rate
            {
                Id = Defaults.RateTwoHundredId,
                Amount = 200,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
            };
            var twoFifty = new Rate
            {
                Id = Defaults.RateTwoFiftyId,
                Amount = 250,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
            };
            var threeHundred = new Rate
            {
                Id = Defaults.RateThreeHundredId,
                Amount = 300,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
            };
            var fiveHundred = new Rate
            {
                Id = Defaults.RateFiveHundredId,
                Amount = 500,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
            };

            builder.HasData(ten, hundred, twoHundred, twoFifty, threeHundred, fiveHundred);
        }
    }
}
