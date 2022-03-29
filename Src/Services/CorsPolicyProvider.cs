using MicroAutomation.Web.Models;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace MicroAutomation.Web.Services
{
    internal class CorsPolicyProvider : ICorsPolicyProvider
    {
        private readonly ILogger _logger;
        private readonly ICorsPolicyProvider _inner;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IConfiguration _configuration;

        public CorsPolicyProvider(
            ILogger<CorsPolicyProvider> logger,
            Decorator<ICorsPolicyProvider> inner,
            IHttpContextAccessor httpContext,
            IConfiguration configuration)
        {
            _logger = logger;
            _inner = inner.Instance;
            _httpContext = httpContext;
            _configuration = configuration;
        }

        public Task<CorsPolicy> GetPolicyAsync(HttpContext context, string policyName)
        {
            var corsConfiguration = new CorsConfiguration();
            _configuration.GetSection(nameof(CorsConfiguration)).Bind(corsConfiguration);

            if (corsConfiguration.CorsPolicyName == policyName)
            {
                return ProcessAsync(context, corsConfiguration);
            }
            else
            {
                return _inner.GetPolicyAsync(context, policyName);
            }
        }

        private async Task<CorsPolicy> ProcessAsync(HttpContext context, CorsConfiguration configuration)
        {
            var origin = GetCorsOrigin(context.Request);
            if (origin != null)
            {
                var path = context.Request.Path;
                _logger.LogDebug("CORS request made for path: {path} from origin: {origin}", path, origin);

                if (configuration.AllowedOrigins == null || configuration.AllowedOrigins.Count == 0)
                {
                    _logger.LogDebug("CorsPolicyService allowed origin: {origin}", origin);
                    return Allow(origin, configuration);
                }

                if (configuration.AllowedOrigins.Any(x => x == origin))
                {
                    _logger.LogDebug("CorsPolicyService allowed origin: {origin}", origin);
                    return Allow(origin, configuration);
                }
                else
                {
                    _logger.LogWarning("CorsPolicyService did not allow origin: {origin}", origin);
                }
            }
            return null;
        }

        private static CorsPolicy Allow(string origin, CorsConfiguration configuration)
        {
            var policyBuilder = new CorsPolicyBuilder()
                .WithOrigins(origin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();

            if (configuration.PreflightCacheDuration.HasValue)
            {
                policyBuilder.SetPreflightMaxAge(configuration.PreflightCacheDuration.Value);
            }

            return policyBuilder.Build();
        }

        private static string GetCorsOrigin(HttpRequest request)
        {
            var origin = request.Headers["Origin"].FirstOrDefault();
            var thisOrigin = request.Scheme + "://" + request.Host;

            // See if the Origin is different than this server's origin. if so
            // that indicates a proper CORS request. some browsers send Origin
            // on POST requests.
            if (origin != null && origin != thisOrigin)
            {
                return origin;
            }

            return null;
        }
    }
}