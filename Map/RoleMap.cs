using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Entities;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Map
{
    public class RoleMap : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            SetupData(builder);
        }
        private void SetupData(EntityTypeBuilder<ApplicationRole> builder)
        {
            var roles = new ApplicationRole[]
            {
                new ApplicationRole
                {
                    Id = RoleHelper.SuperAdmin,
                    Name = RoleHelper.SUPERADMIN.ToString(),
                    NormalizedName = RoleHelper.SUPERADMIN.ToString(),
                },
                new ApplicationRole
                {
                    Id = RoleHelper.Admin,
                    Name = RoleHelper.ADMIN.ToString(),
                    NormalizedName = RoleHelper.ADMIN.ToString(),
                }

            };

            builder.HasData(roles);
        }
    }
}
