using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Constant;
using Optima.Models.Entities;
using Optima.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Map
{
    public class UserRoleMap : IEntityTypeConfiguration<ApplicationUserRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
        {
            SetupUserRoles(builder);
        }
        private void SetupUserRoles(EntityTypeBuilder<ApplicationUserRole> builder)
        {
            builder.HasData(
            new ApplicationUserRole
            {
                UserId = Defaults.SuperAdminId,
                RoleId = RoleHelper.SuperAdmin
            },

            new ApplicationUserRole
            {
                UserId = Defaults.AdminId,
                RoleId = RoleHelper.Admin
            });
        }
    }
}
