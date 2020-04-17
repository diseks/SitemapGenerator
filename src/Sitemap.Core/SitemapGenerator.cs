using Microsoft.Extensions.Logging;
using Sitemap.Core.Config;
using Sitemap.Core.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sitemap.Core
{
    public interface ISitemapGenerator : IDisposable
    {
        Task Generate();
    }

    public class SitemapGenerator : ISitemapGenerator
    {
        private readonly SitemapGeneratorSettings _settings;

        public SitemapGenerator(SitemapGeneratorSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Dispose() { }

        public async Task Generate()
        {
            try
            {
                var totalDataCount = _settings.SiteMapDataSettings.Count;

                if (totalDataCount == 0) { return; }

                var fetchDataTask = new Task<IEnumerable<string>>[totalDataCount];

                for (int i = 0; i < totalDataCount; i++)
                {
                    var mapSettings = _settings.SiteMapDataSettings[i];

                    _settings.Logger.LogInformation($"Create job for table {mapSettings.Table}");

                    var backgroundJob = new BackgroundJob(_settings.Storage,
                        mapSettings,
                        _settings.Serializer,
                        _settings.FileHandlerHelper,
                        _settings.Logger
                        );

                    fetchDataTask[i] = backgroundJob.Process();
                }

                _settings.Logger.LogInformation($"Start all jobs. Waiting results...");

                var result = await Task.WhenAll(fetchDataTask);

                FinalizeSitemap(result);
            }
            catch (Exception ex)
            {
                _settings.Logger.LogError("Generate()", ex);
            }
        }

        private void FinalizeSitemap(IEnumerable<string>[] filesArray)
        {
            _settings.Logger.LogInformation($"Finalize sitemap. Wrap to <sitemapindex>");
            _settings.FileHandlerHelper.Save("sitemap.xml", _settings.Serializer.SerializeSitemap(filesArray));
        }
    }
}
