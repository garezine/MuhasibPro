namespace Muhasib.Data.Managers.UpdataManager
{
    public interface ILocalUpdateManager
    {
        public UpdateSettingsModel UpdateSettings { get; set; }
        public Task<UpdateSettingsModel> LoadAsync();
        public Task SaveAsync(UpdateSettingsModel model);
    }
}
