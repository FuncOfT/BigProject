using AutoMapper;
using BigProject.Models;
using BigProject.Services;
using BigProject.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace BigProject
{
    public class Startup
    {
        private IHostingEnvironment _env;
        private IConfigurationRoot _config;

        public Startup(IHostingEnvironment env)
        {
            _env = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(_env.ContentRootPath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();

            _config = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_config);

            if (_env.IsEnvironment("Development") || _env.IsEnvironment("Testing"))
            {
                services.AddScoped<IMailService, DebugMailService>();
            }

            services.AddIdentity<WorldUser, IdentityRole>(config =>
           {
               config.User.RequireUniqueEmail = true;
               config.Password.RequiredLength = 8;
               config.Cookies.ApplicationCookie.LoginPath = "/Auth/Login";
               config.Cookies.ApplicationCookie.Events = new CookieAuthenticationEvents()
               {
                   OnRedirectToLogin = async ctx =>
                   {
                       if (ctx.Request.Path.StartsWithSegments("/api") &&
                       ctx.Response.StatusCode == 200)
                       {
                           ctx.Response.StatusCode = 401;
                       }
                       else
                       {
                           ctx.Response.Redirect(ctx.RedirectUri);
                       }
                       await Task.Yield();
                   }
               };
           })
            .AddEntityFrameworkStores<WorldContext>();


            services.AddEntityFrameworkSqlServer().AddDbContext<WorldContext>();

            services.AddScoped<IWorldRepository, WorldRepository>();

            services.AddTransient<WorldContextSeedData>();

            services.AddTransient<GeoCoordsService>();

            services.AddLogging();

            services.AddMvc(config =>
            {
                if (_env.IsProduction())
                {
                    config.Filters.Add(new RequireHttpsAttribute());
                }
            })
            .AddJsonOptions(config =>
                {
                    config.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory,
            WorldContextSeedData seeder)
        {
            Mapper.Initialize(config =>
            {
                config.CreateMap<TripViewModel, Trip>().ReverseMap();
                config.CreateMap<StopViewModel, Stop>().ReverseMap();
            });

            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddDebug(LogLevel.Information);
            }
            else
            {
                loggerFactory.AddDebug(LogLevel.Error);
            }

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseMvc(config =>
            {
            config.MapRoute("Default", 
                "{controller}/{action}/{id?}", 
                new { controller = "App", action = "Index" });
            });

            seeder.EnsureSeedData().Wait();
        }
    }
}
