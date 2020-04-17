using Microsoft.Extensions.Configuration;
using Serilog;
using Sitemap.Core;
using Sitemap.Core.Builders;
using Sitemap.Core.Config;
using System;
using System.Threading.Tasks;

namespace Sitemap.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);

            // Load configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            using (var generator = CreateGenerator(configuration))
            {
                await generator.Generate();
            }
        }

        private static ISitemapGenerator CreateGenerator(IConfiguration configuration)
        {
            var connectionString = configuration["ConnectionStrings:DbConnection"];

            var generator = new SitemapGeneratorBuilder()
                .UseStorage(x => x.WithConnectionString(connectionString))
                .WithFilters(() => configuration.GetSection("FilterSettings").Get<SiteMapFilterSettings[]>())
                .WithSerializer(() => configuration.GetSection("SerializerSettings").Get<SerializerSettings>())
                .WithFileHandler(() => configuration.GetSection("FileHandlerSettings").Get<FileHandlerSettings>())
                .WithLogger(builder =>
                {
                    builder.AddSerilog(
                        logger: new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger(),
                        dispose: true);
                })
                .Build();

            return generator;
        }
    }
}
