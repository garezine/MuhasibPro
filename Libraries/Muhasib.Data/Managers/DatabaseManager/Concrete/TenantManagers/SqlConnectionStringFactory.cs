using Microsoft.Data.SqlClient;
using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantManagers
{
    public class SqlConnectionStringFactory : ISqlConnectionStringFactory
    {
        private readonly string _serverName;
        private readonly bool _integratedSecurity;
        private readonly bool _trustServerCertificate;

        public SqlConnectionStringFactory(
            string serverName,
            bool integratedSecurity = true,
            bool trustServerCertificate = true)
        {
            _serverName = serverName;
            _integratedSecurity = integratedSecurity;
            _trustServerCertificate = trustServerCertificate;           
        }

        public string CreateForDatabase(string databaseName)
        {
            return $"Data Source={_serverName};" +
                   $"Integrated Security={_integratedSecurity};" +
                   $"TrustServerCertificate={_trustServerCertificate};" +
                   $"Initial Catalog={databaseName};";
        }
          
        
        public string GetMasterConnectionString()
        {
            var serverCandidates = new[] { "(localdb)\\mssqllocaldb", ".\\SQLEXPRESS", ".", "localhost" };

            foreach (var server in serverCandidates)
            {
                var connectionString = $"Data Source={server};Integrated Security=true;TrustServerCertificate=true;Initial Catalog=master;";
                
                if (TestConnectionAsync(connectionString))
                {                    
                    return connectionString;
                }
            }

            throw new Exception("No SQL Server instance found");
        }
        private bool TestConnectionAsync(string connectionString)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch (Exception)
            {
                throw new Exception("No SQL Server connection test");
            }
        }
    }
}
