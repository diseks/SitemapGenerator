using Sitemap.Core.Dtos;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Sitemap.Core.Storage
{
    public interface IStorage
    {
        IEnumerable<SiteMapEntry> GetEntries(string table, int take = 0, int skip = 0);
    }

    public class SqlServerStorage : IStorage
    {
        private readonly Func<DbConnection> _connectionFactory;
        private readonly string _connectionString;

        public SqlServerStorage(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            _connectionFactory = () => new SqlConnection(_connectionString);
        }

        public IEnumerable<SiteMapEntry> GetEntries(string table, int take = 0, int skip = 0)
        {
            if (string.IsNullOrWhiteSpace(table)) throw new ArgumentNullException(nameof(table));

            var queryString = $@"select Id, DateModified from {table}";

            if (take > 0) 
            {
                queryString += $" order by id OFFSET {skip} ROW FETCH NEXT {take} ROWS ONLY";
            }

            return UseConnection(connection => connection.Query<SiteMapEntry>(queryString));
        }

        private T UseConnection<T>(Func<DbConnection, T> func)
        {
            DbConnection connection = null;

            try
            {
                connection = CreateAndOpenConnection();
                return func(connection);
            }
            finally
            {
                ReleaseConnection(connection);
            }
        }

        private DbConnection CreateAndOpenConnection()
        {

            DbConnection connection = null;

            try
            {
                connection =  _connectionFactory();

                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                return connection;
            }
            catch
            {
                ReleaseConnection(connection);
                throw;
            }
     
        }

        private void ReleaseConnection(IDbConnection connection)
        {
            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }
}
