using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Optima.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Optima.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationUserRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        public DbSet<BankAccount> BankAccounts { get; set; } 
        public DbSet<Notification> Notifications { get; set; } 
        public DbSet<RefreshToken> RefreshTokens { get; set; } 
        public DbSet<Country> Countries { get; set; }
        public DbSet<Denomination> Denominations { get; set; }
        public DbSet<TermsAndCondition> TermsAndConditions { get; set; }
        public DbSet<CardTypeDenomination> CardTypeDenomination { get; set; }
        public DbSet<WalletBalance> WalletBalance { get; set; }
        public DbSet<CreditDebit> CreditDebit { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<Prefix> SpecialPrefixes { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Models.Entities.CardType> CardType { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }


    }
}
