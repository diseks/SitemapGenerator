using Microsoft.Extensions.Logging;
using Sitemap.Core.Config;
using Sitemap.Core.Helpers;
using Sitemap.Core.Storage;
using System;
using System.Collections.Generic;

namespace Sitemap.Core.Builders
{
    public class SitemapGeneratorBuilder
    {
        private SitemapGeneratorSettings _settings = new SitemapGeneratorSettings();

        public SitemapGeneratorBuilder UseStorage(Func<StorageBuilder, IStorage> storageFactory)
        {
            _settings.Storage = storageFactory(new StorageBuilder());
            return this;
        }

        public SitemapGeneratorBuilder WithSerializer(Func<SerializerSettings> serializerBuilder)
        {
            _settings.Serializer = new SitemapVer09XmlSerializer(serializerBuilder());
            return this;
        }

        public SitemapGeneratorBuilder WithFileHandler(Func<FileHandlerSettings> fileHandlerBuilder)
        {
            _settings.FileHandlerHelper = new FileHandlerHelper(fileHandlerBuilder());
            return this;
        }

        public SitemapGeneratorBuilder WithLogger(Action<ILoggingBuilder> configure)
        {
            var loggerFactory = LoggerFactory.Create(configure);

            _settings.Logger = loggerFactory.CreateLogger<SitemapGenerator>();

            return this;
        }

        public SitemapGeneratorBuilder WithFilters(Func<IList<SiteMapFilterSettings>> dataFactory)
        {
            _settings.SiteMapDataSettings = dataFactory();
            return this;
        }

        public ISitemapGenerator Build()
        {
            return new SitemapGenerator(_settings);
        }
    }
}
