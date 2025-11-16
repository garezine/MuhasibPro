namespace Muhasib.Business.Services.Contracts.LogServices
{
    public interface ILogService
    {
        public IAppLogService AppLogService { get; }
        public ISistemLogService SistemLogService { get; }
    }
}
