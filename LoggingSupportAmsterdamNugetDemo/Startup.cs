using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Microsoft.ApplicationInsights.Extensibility;
using DASIT.LoggingSupport;

namespace LoggingSupportAmsterdamNugetDemo
{
    public class Startup
    {
        private readonly ILogger logger;

        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;

            if (Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"] != null)
            {

                var telemConfig = new TelemetryConfiguration(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(Configuration)
                    .Enrich.With<EventTypeEnricher>()
                    .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                        .WithDefaultDestructurers()
                        .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }))
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("EnvironmentName", env.EnvironmentName)
                    .Enrich.WithProperty("ApplicationName", "Logging Support Nuget Demo")
                    .WriteTo.ApplicationInsights(telemConfig, TelemetryConverter.Traces)
                    .CreateLogger();

                logger = Log.Logger.ForContext<Startup>();

            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.With<EventTypeEnricher>()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                        .WithDefaultDestructurers()
                        .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }))
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", "Logging Support Nuget Demo")
                .CreateLogger();

                logger = Log.Logger.ForContext<Startup>();

                logger.Warning("APPINSIGHTS_INSTRUMENTATIONKEY not found in Configuration");

            }

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            logger.AddCallerDetails().Information("ConfigureServices");

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
