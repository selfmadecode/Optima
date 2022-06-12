using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Map
{
    public class CreditDebitMap : IEntityTypeConfiguration<CreditDebit>
    {
        public void Configure(EntityTypeBuilder<CreditDebit> builder)
        {
            //builder.HasOne(x => x.BankAccount).WithOne().OnDelete(DeleteBehavior.NoAction);
        }
    }
}
