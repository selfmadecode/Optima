using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Map
{
    public class VisaPrefixMap : IEntityTypeConfiguration<Prefix>
    {
        public void Configure(EntityTypeBuilder<Prefix> builder)
        {
            SeedPrefix(builder);
        }

        public void SeedPrefix(EntityTypeBuilder<Prefix> builder)
        {
            var One = new Prefix
            {
                Id = Guid.Parse("1631bd64-f684-4893-bb51-f4a8e97af640"),
                PrefixNumber = "4135"
            };

            var two = new Prefix
            {
                Id = Guid.Parse("d0b7dcb4-fc4e-4996-b321-d27915efda54"),
                PrefixNumber = "4153"
            };

            builder.HasData(One, two);
        }
    }
}
