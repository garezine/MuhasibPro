using Muhasib.Data.Managers.DatabaseManager.Contracts.TenantManager;

namespace Muhasib.Data.Managers.DatabaseManager.Concrete.TenantManagers
{
    public class SqlConnectionStringFactory : ISqlConnectionStringFactory
    {
        private readonly string _serverName;
        private readonly bool _integratedSecurity;
        private readonly bool _trustServerCertificate;

        public SqlConnectionStringFactory(
            string serverName = "localhost",
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

        public string CreateForMaster()
        {
            return CreateForDatabase("master");
        }
    }
}
