using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Map
{
    public class BankAccountMap : IEntityTypeConfiguration<BankAccount>
    {
        /// <summary>
        /// Configures the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            //builder.ToTable(nameof(Company));

            //Properties           

            SetUpBankAccount(builder);
        }

        public void SetUpBankAccount(EntityTypeBuilder<BankAccount> builder)
        {
            var bankAccount1 = new BankAccount
            {
                BankName = "JAIZ BANK",
                AccountName = "Williams Mary",
                AccountNumber = "8989898887",
                UserId = Guid.Parse("e7d58c75-18bc-4868-b54d-0a1fdf8fe94d")
            };

            var bankAccount2 = new BankAccount
            {
                BankName = "UNITY BANK",
                AccountName = "Williams Mary",
                AccountNumber = "7645458887",
                UserId = Guid.Parse("e7d58c75-18bc-4868-b54d-0a1fdf8fe94d")
            };

            var bankAccounts = new List<BankAccount> { bankAccount1, bankAccount2 };
            builder.HasData(bankAccounts);

        }
    }

    
}
