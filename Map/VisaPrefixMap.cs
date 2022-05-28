using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Map
{
    public class VisaPrefixMap : IEntityTypeConfiguration<VisaPrefix>
    {
        public void Configure(EntityTypeBuilder<VisaPrefix> builder)
        {
            SeedPrefix(builder);
        }

        public void SeedPrefix(EntityTypeBuilder<VisaPrefix> builder)
        {
            var One = new VisaPrefix
            {
                Prefix = "4135"
            };

            var two = new VisaPrefix
            {
                Prefix = "4153"
            };

            builder.HasData(One, two);
        }
    }
}
