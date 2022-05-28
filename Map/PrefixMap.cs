using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Map
{
    public class PrefixMap : IEntityTypeConfiguration<Prefix>
    {
        public void Configure(EntityTypeBuilder<Prefix> builder)
        {
            SeedPrefix(builder);
        }

        public void SeedPrefix(EntityTypeBuilder<Prefix> builder)
        {
            var One = new Prefix
            {
                PrefixNumber = "4135",
                Id = Guid.Parse("fbb2ffe7-b56d-4ad8-abf4-92331f30872e")
            };

            var two = new Prefix
            {
                PrefixNumber = "4153",
                Id = Guid.Parse("1569a554-0faf-468c-be89-2584d5992c86")
            };

            builder.HasData(One, two);
        }
    }
}
