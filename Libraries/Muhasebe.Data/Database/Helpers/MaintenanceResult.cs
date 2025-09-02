namespace Muhasebe.Data.Database.Helpers
{
    public class MaintenanceResult
    {
        public MaintenanceResult(bool success, string operationName, string details = null, string errorMessage = null)
        {
            Success = success;
            OperationName = operationName;
            Details = details;
            ErrorMessage = errorMessage;
        }

        public bool Success { get; }
        public string OperationName { get; }
        public string Details { get; }
        public string ErrorMessage { get; }
    }
}
