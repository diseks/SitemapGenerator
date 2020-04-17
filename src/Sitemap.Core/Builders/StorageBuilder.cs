using Sitemap.Core.Storage;
using System;

namespace Sitemap.Core.Builders
{
    public class StorageBuilder
    {
        public IStorage WithConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) { throw new ArgumentNullException(nameof(connectionString)); }

            return new SqlServerStorage(connectionString);
        }
    }
}
