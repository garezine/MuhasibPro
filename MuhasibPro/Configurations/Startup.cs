using CommunityToolkit.Mvvm.DependencyInjection;
using Muhasebe.Data.Database.SistemDatabase;
using Muhasebe.Data.Helper;

namespace MuhasibPro.Configurations
{
    public class Startup
    {
        private static readonly Lazy<Startup> _instance = new Lazy<Startup>(() => new Startup());


        public static Startup Instance => _instance.Value;

        private Startup()
        {
        } 
   
    }
}
