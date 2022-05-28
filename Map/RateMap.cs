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
    public class RateMap : IEntityTypeConfiguration<Denomination>
    {
        public void Configure(EntityTypeBuilder<Denomination> builder)
        {
            SetUpRate(builder);
        }
        public void SetUpRate(EntityTypeBuilder<Denomination> builder)
        {
            var ten = new Denomination
            {
                Id = Defaults.RateTenId,
                Amount = 10,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
            };
            var hundred = new Denomination
            {
                Id = Defaults.RateHundredId,
                Amount = 100,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
            };
            var twoHundred = new Denomination
            {
                Id = Defaults.RateTwoHundredId,
                Amount = 200,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
            };
            var twoFifty = new Denomination
            {
                Id = Defaults.RateTwoFiftyId,
                Amount = 250,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
            };
            var threeHundred = new Denomination
            {
                Id = Defaults.RateThreeHundredId,
                Amount = 300,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
            };
            var fiveHundred = new Denomination
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
