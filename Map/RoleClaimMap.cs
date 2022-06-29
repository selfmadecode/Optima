using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Entities;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Optima.Utilities.Helpers.PermisionProvider;

namespace Optima.Map
{
    public class RoleClaimMap : IEntityTypeConfiguration<ApplicationRoleClaim>
    {
        private static int counter = 0;

        public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
        {
            SetUpAdminRoleClaim(builder);
            SetUpSuperAdminRoleClaim(builder);
        }
        private void SetUpSuperAdminRoleClaim(EntityTypeBuilder<ApplicationRoleClaim> builder)
        {
            var superAdminPermission = PermisionProvider.SuperAdminPermission();

            foreach (var permission in superAdminPermission)
            {
                builder.HasData(new ApplicationRoleClaim
                {
                    Id = ++counter,
                    ClaimValue = permission.ToString(),
                    ClaimType = nameof(Permission),
                    RoleId = RoleHelper.SuperAdmin
                });

            }
        }

        private void SetUpAdminRoleClaim(EntityTypeBuilder<ApplicationRoleClaim> builder)
        {
            var superAdminPermission = PermisionProvider.AdminPermission();

            foreach (var permission in superAdminPermission)
            {
                builder.HasData(new ApplicationRoleClaim
                {
                    Id = ++counter,
                    ClaimValue = permission.ToString(),
                    ClaimType = nameof(Permission),
                    RoleId = RoleHelper.Admin
                });
            }
        }
        
    }
}
