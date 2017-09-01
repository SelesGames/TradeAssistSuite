using Microsoft.Extensions.Configuration;
using System;
using System.Threading;

namespace TradeMonitor.WebJob.Poloniex
{
    class Program
    {
        static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            ConfigureEnvironment();
            var endpoint = Configuration.GetConnectionString("endpoint");
            var authKey = Configuration.GetConnectionString("authKey");

            DocumentStorageHelpers.DbClientFactory.Current.SetCredentials(endpoint, authKey);

            ManualResetEvent quitEvent = new ManualResetEvent(false);

            var listener = new Listener();
            listener.Start().Wait();
            Console.WriteLine("Poloniex trade listener started");
            quitEvent.WaitOne();
        }

        // below method via: https://stackoverflow.com/questions/39573571/net-core-console-application-how-to-configure-appsettings-per-environment
        static void ConfigureEnvironment()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                //.AddJsonFile($"appsettings.json", true, true)  // dev
                .AddJsonFile($"appsettings.prod.json", true, true)  // prod
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            /* what this looks like in an asp.net core app:
                
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build(); */
        }
    }
}
 