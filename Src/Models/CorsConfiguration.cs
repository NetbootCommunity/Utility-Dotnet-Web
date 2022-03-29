using System;
using System.Collections.Generic;

namespace MicroAutomation.Web.Models
{
    public class CorsConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the cors policy.
        /// </summary>
        /// <value>
        /// The name of the cors policy.
        /// </value>
        public string CorsPolicyName { get; set; } = "MicroAutomation";

        /// <summary>
        /// The value to be used in the preflight `Access-Control-Max-Age` response header.
        /// </summary>
        public TimeSpan? PreflightCacheDuration { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Gets or sets the list of allowed origins.
        /// </summary>
        public List<string> AllowedOrigins { get; set; }
    }
}