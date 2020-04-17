using Sitemap.Core.Config;
using Sitemap.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sitemap.Core.Helpers
{
    public interface ISerializer
    {
        string SerializeSitemap(IEnumerable<string>[] fileNames);
        string SerializeUrlSet(SiteMapEntry item, string template);
        string PrepareUrlSetXml(string content);
    }

    public class SitemapVer09XmlSerializer : ISerializer
    {
        private const string XmlNs = "http://www.sitemaps.org/schemas/sitemap/0.9";

        private readonly SerializerSettings _settings;

        public SitemapVer09XmlSerializer(SerializerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string SerializeSitemap(IEnumerable<string>[] fileNames)
        {
            var content = new StringBuilder($"<sitemapindex xmlns=\"{XmlNs}\">");

            for (int i = 0; i < fileNames.Length; i++)
            {
                foreach (var file in fileNames[i])
                {
                    content.Append($"<sitemap>" +
                                    $"<loc>{string.Format("{0}/{1}", _settings.Host, file)}</loc>" +
                                    $"<lastmod>{DateTime.Now.ToString("s")}</lastmod>" +
                                    $"<changefreq>daily</changefreq>" +
                                    $"<priority>0.5</priority>" +
                                    $"</sitemap>");
                }
            }

            content.Append("</sitemapindex>");

            return content.ToString();
        }

        public string SerializeUrlSet(SiteMapEntry item, string template)
        {
            return $"<url>" +
                $"<loc>{string.Concat(_settings.Host, template.Replace("{id}", item.Id.ToString()))}</loc>" +
                $"<lastmod>{item.DateModified.ToString("s")}</lastmod>" +
                $"<changefreq>daily</changefreq>" +
                $"<priority>0.5</priority>" +
                $"</url>";
        }

        public string PrepareUrlSetXml(string content)
        {
            return $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"{XmlNs}\">{content}</urlset>";
        }
    }
}
