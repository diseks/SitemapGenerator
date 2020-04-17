using Sitemap.Core.Config;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sitemap.Core.Helpers
{
    public interface IFileHandlerHelper
    {
        void Save(string fileName, string content);
    }

    public class FileHandlerHelper : IFileHandlerHelper
    {
        private readonly FileHandlerSettings _settings;

        public FileHandlerHelper(FileHandlerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Save(string fileName, string content)
        {
            var path = Path.Combine(_settings.DestinationFolderPath, fileName);
            File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}
