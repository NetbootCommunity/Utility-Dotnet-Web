using MicroAutomation.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace MicroAutomation.Web.Extensions
{
    public static class KestrelExtensions
    {
        public static void ConfigureEndpoints(this KestrelServerOptions options)
        {
            var environment = options.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            options.AddServerHeader = false;

            var serviceConfig = new ServiceOption();
            configuration.Build().GetSection(nameof(ServiceOption)).Bind(serviceConfig);
            if (serviceConfig.Authentication == AuthenticationType.Certificate)
            {
                options.ConfigureHttpsDefaults(opt =>
                {
                    opt.CheckCertificateRevocation = true;
                    opt.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                    opt.SslProtocols = SslProtocols.Tls12;
                });
            }

            foreach (var endpoint in serviceConfig.Endpoints)
            {
                var port = endpoint.Port ?? (endpoint.Scheme == "https" ? 443 : 80);
                options.Listen(IPAddress.Any, port,
                    listenOptions =>
                    {
                        if (endpoint.Scheme == "https")
                        {
                            try
                            {
                                var certificate = LoadCertificate(endpoint, environment);
                                listenOptions.UseHttps(certificate);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Unable to load certificat : {ex.Message}");
                            }
                        }
                    });
            }
        }

        private static X509Certificate2 LoadCertificate(ServiceEndpointOption config, IWebHostEnvironment environment)
        {
            if (config.StoreName != null && config.StoreLocation != null)
            {
                using (var store = new X509Store(config.StoreName, Enum.Parse<StoreLocation>(config.StoreLocation)))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certificate = store.Certificates.Find(
                        X509FindType.FindBySubjectName,
                        config.Subject,
                        validOnly: environment.EnvironmentName != "Development");

                    if (certificate.Count == 0)
                    {
                        throw new InvalidOperationException($"Certificate not found for {config.Subject}.");
                    }

                    return certificate[0];
                }
            }

            if (config.FilePath != null && config.Password != null)
            {
                return new X509Certificate2(config.FilePath, config.Password);
            }
            else if (config.FilePath != null && config.Password == null)
            {
                return new X509Certificate2(config.FilePath);
            }

            throw new InvalidOperationException("No valid certificate configuration found for the current endpoint.");
        }
    }
}