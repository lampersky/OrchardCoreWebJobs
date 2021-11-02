using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace Repro.WebJobs
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .UseEnvironment("Development")
                .ConfigureAppConfiguration((builderContext, configurationBinder) =>
                {
                    configurationBinder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((builderContext, services) =>
                {
                    ConfigureServices(services, builderContext.Configuration);
                })
                .ConfigureWebJobs((hostBuilderContext, webJobsBuilder) =>
                {
                    webJobsBuilder.AddAzureStorageCoreServices();
                    webJobsBuilder.AddTimers();
                    //uncomment when 'AzureWebJobsStorage' connection string is valid
                    //webJobsBuilder.AddAzureStorageQueues();
                })
                .ConfigureLogging((hostBuilderContext, loggingBuilder) =>
                {
                    loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                    loggingBuilder.AddConsole();
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }

        private static void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOrchardCms();
        }
    }
}
