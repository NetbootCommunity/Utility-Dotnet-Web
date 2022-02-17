using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MicroAutomation.Web.Extensions;

public static class HstsExtensions
{
    /// <summary>
    /// Add HSTS service.
    /// </summary>
    /// <param name="services"></param>
    public static void AddHsts(this IServiceCollection services)
    {
        services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(90);
            options.IncludeSubDomains = true;
            options.Preload = true;
        });
    }
}