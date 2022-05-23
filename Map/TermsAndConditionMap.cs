using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Map
{
    public class TermsAndConditionMap : IEntityTypeConfiguration<TermsAndCondition>
    {
        public void Configure(EntityTypeBuilder<TermsAndCondition> builder)
        {
            UploadTermsAndCondition(builder);
        }

        public void UploadTermsAndCondition(EntityTypeBuilder<TermsAndCondition> builder)
        {
            var tnc = new TermsAndCondition
            {
                Id = Guid.Parse("4a61fb4d-f1d1-460d-b377-6cd8696b585c"),
                TermsAndConditions = "You accept the terms of use",
                CreatedOn = DateTime.UtcNow,
            };

            builder.HasData(tnc);
        }
    }

}
