using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Map
{
    public class ReceiptMap : IEntityTypeConfiguration<Receipt>
    {
        public void Configure(EntityTypeBuilder<Receipt> builder)
        {
            SeedReceipt(builder);
        }

        public void SeedReceipt(EntityTypeBuilder<Receipt> builder)
        {
           var activationReceipt = new Receipt
           {
               Name = "Activation Receipt".ToUpper(),
               Id = Guid.Parse("8798758d-765d-47e8-a9e5-bf0c8f11fab3")
           };

            var cashReceipt = new Receipt
            {
                Name = "Cash Receipt".ToUpper(),
                Id = Guid.Parse("b1dce9bf-3f07-4417-9046-51508972f3b2")
            };

            var debitReceipt = new Receipt
            {
                Name = "Debit Receipt".ToUpper(),
                Id = Guid.Parse("fc1ec7c0-586c-45b0-be12-9b3c2be13d82")
            };

            var noReceipt = new Receipt
            {
                Name = "No Receipt".ToUpper(),
                Id = Guid.Parse("f98771da-80ea-4cc7-8598-9d2f230551f7")
            };

            builder.HasData(activationReceipt, cashReceipt, debitReceipt, noReceipt);
        }
    }
}
