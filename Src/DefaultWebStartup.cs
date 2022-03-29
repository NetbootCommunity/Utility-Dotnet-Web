using MicroAutomation.Web.Extensions;
using MicroAutomation.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Netboot.Cache.Memory;

namespace MicroAutomation.Web
{
    public abstract class DefaultWebStartup
    {
        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        protected DefaultWebStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configures services for the application.
        /// </summary>
        /// <param name="services">The collection of services to configure the application with.</param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            // Configure HSTS
            // The default HSTS value is 90 days.
            // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            // https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?WT.mc_id=DT-MVP-5003978#http-strict-transport-security-protocol-hsts
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security
            services.AddHsts();

            // Cors
            services.AddCors();
            services.AddTransientDecorator<ICorsPolicyProvider, CorsPolicyProvider>();

            // Configure distributed cache
            services.AddTypedMemoryCache();

            // Frameworks
            services.AddHttpContextAccessor();
        }

        /// <summary>
        /// This method gets called by the runtime.
        /// Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Apply minimal configuration.
            MinimalConfigure(app, env);
        }

        /// <summary>
        /// Method to apply a minimum configuration for the runtime.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void MinimalConfigure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Checks if the current host environment name is development.
            if (env.IsDevelopment() || env.EnvironmentName == "LocalDevelopment")
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            // Adds a CORS middleware.
            app.ConfigureCors(Configuration);

            // Enables routing capabilities.
            app.UseRouting();
        }
    }
}