using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Optima.Context;
using Optima.Models.Config;
using Optima.Models.Entities;
using Optima.Services.Implementation;
using Optima.Services.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Optima
{
    public partial class Startup
    {
        
        public void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.DescribeAllParametersInCamelCase();
                c.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Optima API" });
            });
        }

        public void ConfigureEntityFrameworkDbContext(IServiceCollection services)
        {
            string dbConnectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    dbConnectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        }

        public void AddIdentityProvider(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, ApplicationUserRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Password.RequiredUniqueChars = 1;


            }).AddEntityFrameworkStores<ApplicationDbContext>();            
            //.AddDefaultTokenProviders();

            //services.Configure<DataProtectionTokenProviderOptions>(options =>
            //{
            //    options.TokenLifespan = TimeSpan.FromHours(24);
            //});
        }

        public void ConfigureDIService(IServiceCollection services)
        {
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(HostingEnvironment.ContentRootPath, Configuration.GetValue<string>("FilePath"))));

            services.AddScoped<IBankAccountService, BankAccountService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.Configure<SmtpConfigSettings>(Configuration.GetSection("SmtpConfig"));
        }
    }
}
