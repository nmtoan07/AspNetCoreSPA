﻿using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreSPA.Common.Entities;
using AspNetCoreSPA.EntityFramework;
using AspNetCoreSPA.Web.Configurations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCoreSPA.Web
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                    o => o.MigrationsAssembly("AspNetCoreSPA.Web")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services.ReplaceDefaultViewEngine();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseStaticFiles();

            //app.UseIdentity();

            app.UseMyIdentity();

            app.UseMvcWithDefaultRoute();

            //CreateSampleData(app.ApplicationServices);
        }

        private static async Task CreateSampleData(IServiceProvider applicationServices)
        {
            using (var dbContext = applicationServices.GetService<ApplicationDbContext>())
            {
                var sqlServerDatabase = dbContext.Database;
                if (sqlServerDatabase != null)
                {
                    if (await sqlServerDatabase.EnsureCreatedAsync())
                    {
                        // add some users
                        var userManager = applicationServices.GetService<UserManager<ApplicationUser>>();

                        // add editor user
                        var stephen = new ApplicationUser
                        {
                            UserName = "Stephen"
                        };
                        var result = await userManager.CreateAsync(stephen, "P@ssw0rd");
                        await userManager.AddClaimAsync(stephen, new Claim("CanEdit", "true"));

                        // add normal user
                        var bob = new ApplicationUser
                        {
                            UserName = "Bob"
                        };
                        await userManager.CreateAsync(bob, "P@ssw0rd");
                    }

                }
            }
        }
    }
}
