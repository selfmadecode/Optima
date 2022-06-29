using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optima.Models.Constant;
using Optima.Models.Entities;
using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Map
{
    public class UserMap : IEntityTypeConfiguration<ApplicationUser>
    {
        public PasswordHasher<ApplicationUser> Hasher { get; set; } = new PasswordHasher<ApplicationUser>();

        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            SetupUsers(builder);
        }
        private void SetupUsers(EntityTypeBuilder<ApplicationUser> builder)
        {
            var sysUser = new ApplicationUser
            {                
                FullName = "Optima User",
                Id = Defaults.SysUserId,
                LastLoginDate = DateTime.Now,
                Email = Defaults.SysUserEmail,
                HasAcceptedTerms = true,
                EmailConfirmed = true,
                NormalizedEmail = Defaults.SysUserEmail.ToUpper(),
                PhoneNumber = Defaults.SysUserMobile,
                UserName = Defaults.SysUserEmail,
                NormalizedUserName = Defaults.SysUserEmail.ToUpper(),
                TwoFactorEnabled = false,
                PhoneNumberConfirmed = true,
                PasswordHash = Hasher.HashPassword(null, "optimA_1"),
                SecurityStamp = "99ae0c45-d682-4542-9ba7-1281e471916b",
                UserType = UserTypes.USER,
            };

            var superAdmin = new ApplicationUser
            {
                FullName = "Optima SuperAdmin",
                Id = Defaults.SuperAdminId,
                LastLoginDate = DateTime.Now,
                Email = Defaults.SuperAdminEmail,
                EmailConfirmed = true,
                HasAcceptedTerms = true,
                NormalizedEmail = Defaults.SuperAdminEmail.ToUpper(),
                PhoneNumber = Defaults.SuperAdminMobile,
                UserName = Defaults.SuperAdminEmail,
                NormalizedUserName = Defaults.SuperAdminEmail.ToUpper(),
                TwoFactorEnabled = false,
                PhoneNumberConfirmed = true,
                PasswordHash = Hasher.HashPassword(null, "optimA_1"),
                SecurityStamp = "016020e3-5c50-40b4-9e66-bba56c9f5bf2",
                UserType = UserTypes.SUPER_ADMIN,
            };
            var admin = new ApplicationUser
            {
                FullName = "Optima Admin",
                Id = Defaults.AdminId,
                LastLoginDate = DateTime.Now,
                Email = Defaults.AdminEmail,
                EmailConfirmed = true,
                HasAcceptedTerms = true,
                NormalizedEmail = Defaults.AdminEmail.ToUpper(),
                PhoneNumber = Defaults.AdminMobile,
                UserName = Defaults.AdminEmail,
                NormalizedUserName = Defaults.AdminEmail.ToUpper(),
                TwoFactorEnabled = false,
                PhoneNumberConfirmed = true,
                PasswordHash = Hasher.HashPassword(null, "optimA_1"),
                SecurityStamp = "016020e3-5c50-40b4-9e66-bba56c9f5bf2",
                UserType = UserTypes.ADMIN,
            };

            builder.HasData(sysUser, superAdmin, admin);
        }
    }
}
