using System;
using EFTestApp.Data;
using EFTestApp.Data.SeedWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace EFTestApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "EfCorePerformance", Version = "v1"}); });

            services.AddDbContext<MsSqlDbContext>(builder => builder
                .UseSqlServer(Configuration.GetConnectionString("MsSql"), sqlOptions =>
                {
                    sqlOptions.ExecutionStrategy(c =>
                        new CustomExecutionStrategy(c, 2, TimeSpan.FromSeconds(30)));
                    // sqlOptions.EnableRetryOnFailure(
                    //     maxRetryCount: 2,
                    //     maxRetryDelay: TimeSpan.FromSeconds(30),
                    //     errorNumbersToAdd: null);
                }));
            
            services.AddDbContext<MySqlDbContext>(builder => builder
                .UseMySql(Configuration.GetConnectionString("MySql"), 
                    ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EfCorePerformance v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}