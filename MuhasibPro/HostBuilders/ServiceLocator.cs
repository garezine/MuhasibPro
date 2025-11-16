using Microsoft.Extensions.DependencyInjection;
using MuhasibPro.Helpers;
using System.Collections.Concurrent;


namespace MuhasibPro.HostBuilders
{
    public class ServiceLocator : IDisposable
    {
        private static readonly ConcurrentDictionary<int, ServiceLocator> _serviceLocators = new();
        private static IServiceProvider _rootServiceProvider = null!;
        private IServiceScope _serviceScope = null;

        private ServiceLocator()
        {
            _serviceScope = _rootServiceProvider.CreateScope();
        }
        public static void Configure(IServiceProvider serviceProvider)
        {
            _rootServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        static public ServiceLocator Current
        {
            get
            {
                int currentViewId = CustomWindowHelper.GetActiveWindowId();
                return _serviceLocators.GetOrAdd(currentViewId, key => new ServiceLocator());
            }
        }
        public static void DisposeCurrent()
        {
            int currentViewId = CustomWindowHelper.GetActiveWindowId();
            if (_serviceLocators.TryRemove(currentViewId, out ServiceLocator current))
            {
                current.Dispose();
            }
        }
        public T GetService<T>()
        {
            return GetService<T>(true);
        }

        public T GetService<T>(bool isRequired)
        {
            if (isRequired)
            {
                return _serviceScope.ServiceProvider.GetRequiredService<T>();
            }
            return _serviceScope.ServiceProvider.GetService<T>();
        }


        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_serviceScope != null)
                {
                    _serviceScope.Dispose();
                }
            }
        }
        #endregion
    }
}
