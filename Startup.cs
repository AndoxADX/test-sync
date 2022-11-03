using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
// using Pomelo.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Store;
using Microsoft.Extensions.Hosting;
namespace TodoApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration,Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            Configuration = configuration;
             var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(env.ContentRootPath + "/appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile(env.ContentRootPath + $"/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false)
                .AddJsonFile(env.ContentRootPath + "/config.json", optional: true, reloadOnChange: false);

            Configuration = builder.Build();
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public Microsoft.AspNetCore.Hosting.IHostingEnvironment Env { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var corsOrigins = Configuration.GetSection("Origins")
               .GetSection(Env.EnvironmentName)
               .AsEnumerable()
               .Where(p => p.Value != null)
               .Select(p => p.Value)
               .ToArray();

            string sqlConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ?? Configuration.GetConnectionString(Env.EnvironmentName);
            string migrationString = Configuration.GetSection("Migrations")[Env.EnvironmentName];

            services.AddDbContext<TodoContext>(options =>
              MySqlDbContextOptionsExtensions.UseMySql(options, sqlConnectionString,
                    b => b.MigrationsAssembly(migrationString)));
        //     services.AddCors(options =>
        //    {
        //        options.AddPolicy("AllowSpecificOrigin",
        //            builder => builder.WithOrigins(corsOrigins)
        //            .AllowAnyHeader()
        //            .AllowAnyMethod());
        //    });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            // Enable this for obtaining HttpContext within other components
            services.AddHttpContextAccessor();
            services.AddScoped<TestStore>();
            services.AddSingleton<IHostedService, SchedulerService>();
            services.AddSingleton<TrxScheduler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
