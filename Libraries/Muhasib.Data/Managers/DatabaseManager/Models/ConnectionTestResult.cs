namespace Muhasib.Data.Managers.DatabaseManager.Models
{
    public enum ConnectionTestResult
    {
        Success,
        SqlServerUnavailable,
        DatabaseNotFound,
        ConnectionFailed,
        UnknownError
    }
}
