using MicroAutomation.Log.Extensions;
using MicroAutomation.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MicroAutomation.Web;

public static class DefaultWebProgram
{
    public static IHost Build<T>(string[] args) where T : DefaultWebStartup
        => CreateHostBuilder<T>(args).Build();

    public static void RunProgram(this IHost host)
    {
        try
        {
            host.Run();
        }
        catch (Exception ex)
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            var pathContextRoot = new FileInfo(location.AbsolutePath).Directory.FullName;
            var logDirectory = Path.Join(pathContextRoot, "Logs");
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            var message = $"[{DateTime.Now}] Host terminated unexpectedl: error: {ex.Message} innerError:{ex.InnerException?.Message}";
            File.AppendAllText($@"{logDirectory}\log-{DateTime.Now:yyyyMMdd}.txt", message);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public static IHostBuilder CreateHostBuilder<TStartup>(string[] args) where TStartup : DefaultWebStartup
    {
        //
        var builder = Host.CreateDefaultBuilder(args);
        IConfiguration configuration = null;

        // Set current directory
        // Based on https://github.com/Topshelf/Topshelf/issues/473
        var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
        var pathContextRoot = new FileInfo(location.AbsolutePath).Directory.FullName;
        builder.UseContentRoot(pathContextRoot);
        Directory.SetCurrentDirectory(pathContextRoot);

        // Add serilog implementation.
        builder.UseCustomSerilog();

        // Sets up the configuration for the remainder of the build process and application.
        builder.ConfigureAppConfiguration((hostContext, config) =>
        {
            // Retrieve the name of the environment.
            var aspnetcore = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var dotnetcore = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var environmentName = string.IsNullOrWhiteSpace(aspnetcore) ? dotnetcore : aspnetcore;
            if (string.IsNullOrWhiteSpace(environmentName))
                environmentName = "Production";

            // Define the configuration builder.
            config.SetBasePath(pathContextRoot);
            config.AddJsonFile("appsettings.json", optional: false);
            config.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
            config.AddEnvironmentVariables();
            config.AddCommandLine(args);

            configuration = config.Build();
        });

        // Configures a HostBuilder with defaults for hosting a web app.
        builder.ConfigureWebHostDefaults(webBuilder =>
        {
            // Add kestrel configuration if runtime is standalone type.
            // Because IIS injects its own configuration.
            if (!IsUnderWindowsWebServer() && !IsUnderDocker())
                webBuilder.UseKestrel(options => options.ConfigureEndpoints());
            webBuilder.UseStartup<TStartup>();
        });

        // Sets the host lifetime configuration.
        builder.AddServiceSupport();

        return builder;
    }

    /// <summary>
    /// Sets the host lifetime, provides notification messages for application started and stopping.
    /// </summary>
    /// <param name="builder"></param>
    private static void AddServiceSupport(this IHostBuilder builder)
    {
        // Add windows service support if runtime is standalone type.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !IsUnderWindowsWebServer())
            builder.UseWindowsService();
        // In docker use the environement variable to configure kestrel
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !IsUnderDocker())
            builder.UseSystemd();
    }

    /// <summary>
    /// Detect if runtime is running on Windows web server.
    /// </summary>
    /// <returns></returns>
    private static bool IsUnderWindowsWebServer()
    {
        var currentProcess = Process.GetCurrentProcess();
        return string.CompareOrdinal(currentProcess.ProcessName, "iisexpress") == 0
            || string.CompareOrdinal(currentProcess.ProcessName, "w3wp") == 0;
    }

    /// <summary>
    /// Detect if runtime is running on docker server.
    /// </summary>
    /// <returns></returns>
    private static bool IsUnderDocker()
        => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
}