namespace MuhasibPro.ViewModels.Common
{
    public static class SingleInstanceViewModels
    {
        public static readonly HashSet<string> _singleViewModel = new()
        {
            Settings,
            FirmaDetail

        };
        private const string Settings = "SettingsViewModel";
        private const string FirmaDetail = "FirmaDetailsViewModel";
    }
}
