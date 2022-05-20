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
    public class CountryMap : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            SetupCountry(builder);
        }
        public void SetupCountry(EntityTypeBuilder<Country> builder)
        {
            var uk = new Country
            {
                Name = "UK",
                Id = Defaults.UK,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };
            var us = new Country
            {
                Name = "UK",
                Id = Defaults.US,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };
            var canada = new Country
            {
                Name = "UK",
                Id = Defaults.Canada,
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };

            builder.HasData(uk, us, canada);
        }
    }
}
