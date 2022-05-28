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
                Id = Guid.Parse("1631bd64-f684-4893-bb51-f4a8e97af640"),
                Prefix = "4135"
            };

            var two = new VisaPrefix
            {
                Id = Guid.Parse("d0b7dcb4-fc4e-4996-b321-d27915efda54"),
                Prefix = "4153"
            };

            builder.HasData(One, two);
        }
    }
}
