using Sitemap.Core.Helpers;
using Sitemap.Core.Storage;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Sitemap.Core.Config
{
    public class SitemapGeneratorSettings
    {
        public ILogger Logger { get; set; }
        public IStorage Storage { get; set; }
        public ISerializer Serializer { get; set; }
        public IList<SiteMapFilterSettings> SiteMapDataSettings { get; set; }

        public FileHandlerHelper FileHandlerHelper { get; set; }
    }
}
