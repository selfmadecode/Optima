using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Optima.Context;
using Optima.Models.Config;
using Optima.Models.DTO;
using Optima.Models.DTO.NotificationDTO;
using Optima.Models.Entities;
using Optima.Services.Implementation;
using Optima.Services.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optima
{
    public partial class Startup
    {
        
        public void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                       new OpenApiInfo
                       {
                           Title = "OPTIMA API",
                           Version = "v1",
                           Description = "Optima Deals Platform",
                           Contact = new OpenApiContact
                           {
                               Name = "OPTIMA",
                               Email = "Info@optima.com"
                           },
                           License = new OpenApiLicense
                           {
                               Name = "MIT License",
                               Url = new Uri("https://en.wikipedia.org/wiki/MIT_Lincense")
                           }
                       });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using Bearer scheme. \r\n\r\n" +
                    "Enter 'Bearer' [space] and then your token in the input below.\r\n\r\n" +
                    "Example: \"Bearer 123456\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }

                });

                c.DescribeAllParametersInCamelCase();
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


            }).AddEntityFrameworkStores<ApplicationDbContext>()       
            .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(24);
            });
        }

        public void ConfigureJWTAuthentication(IServiceCollection services)
        {
            var appSettingsSection = Configuration.GetSection("JWT");
            services.Configure<Jwt>(appSettingsSection);

            var appSettings = appSettingsSection.Get<Jwt>();
            var secret = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = true;
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = appSettings.ValidIssuer,
                    ValidAudience = appSettings.ValidAudience,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secret)
                };

                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/api")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }

        public void ConfigureDIService(IServiceCollection services)
        {
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(HostingEnvironment.ContentRootPath, Configuration.GetValue<string>("FilePath"))));
            services.AddSignalR();

            services.AddScoped<IBankAccountService, BankAccountService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IEncrypt, EncryptService>();
            services.AddScoped<IDenominationService, DenominationService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ITermsService, TermsService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            services.AddScoped<ICardService, CardService>();

            services.Configure<SmtpConfigSettings>(Configuration.GetSection("SmtpConfig"));

            services.Configure<FcmNotification>(Configuration.GetSection("FcmNotification"));

            services.Configure<EmailLinkDTO>(options =>
             Configuration.GetSection(nameof(EmailLinkDTO)).Bind(options));
        }
    }
}
