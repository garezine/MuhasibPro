using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasib.Data.BaseRepositories;
using Muhasib.Data.BaseRepositories.Contracts;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.Contracts.SistemRepositories.Authentication;
using Muhasib.Data.DataContext;
using Muhasib.Data.Repositories.SistemRepositories;
using Muhasib.Data.Repositories.SistemRepositories.Authentications;
using Muhasib.Domain.Entities.SistemEntity;


namespace Muhasib.Business.HostBuilders
{
    public static class AddRepositoryHostBuilderExtensions
    {
        public static IHostBuilder AddRepositories(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                services.AddSingleton<IUnitOfWork<SistemDbContext>, UnitOfWork<SistemDbContext>>();
                services.AddSingleton<IUnitOfWork<AppDbContext>, UnitOfWork<AppDbContext>>();
                services.AddSingleton<IPasswordHasher<Kullanici>>(provider => new PasswordHasher<Kullanici>());

                ////Repository
                //services.AddScoped<IGenericRepository<SistemLog>, GenericRepository<SistemLog>>();
                //services.AddScoped<IGenericRepository<AppLog>, GenericRepository<AppLog>>();
                //services.AddScoped<IGenericRepository<Firma>, GenericRepository<Firma>>();
                //services.AddScoped<IGenericRepository<Kullanici>, GenericRepository<Kullanici>>();

                services.AddSingleton<ISistemLogRepository, SistemLogRepository>();
                services.AddSingleton<IAppLogRepository, AppLogRepository>();
                services.AddSingleton<IUserRepository, UserRepository>();
                services.AddSingleton<IAuthenticationRepository, AuthenticationRepository>();
                services.AddSingleton<IAccountStore, AccountStore>();
                services.AddSingleton<IAuthenticator, Authenticator>();
                services.AddSingleton<IFirmaRepository, FirmaRepository>();
                services.AddSingleton<IMaliDonemRepository, MaliDonemRepository>();
                services.AddSingleton<IMaliDonemDbRepository, MaliDonemDbRepository>();


            });
            return host;
        }
    }
}
