namespace MuhasibPro.ViewModels.Infrastructure.Common
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; } = new();
    }
}
