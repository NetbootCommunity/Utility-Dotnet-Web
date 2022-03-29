using MicroAutomation.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace MicroAutomation.Web.Extensions
{
    public static class CorsExtensions
    {
        public static void ConfigureCors(this IApplicationBuilder app, IConfiguration configuration)
        {
            var corsConfiguration = new CorsConfiguration();
            configuration.GetSection(nameof(CorsConfiguration)).Bind(corsConfiguration);
            app.UseCors(corsConfiguration.CorsPolicyName);
        }
    }
}