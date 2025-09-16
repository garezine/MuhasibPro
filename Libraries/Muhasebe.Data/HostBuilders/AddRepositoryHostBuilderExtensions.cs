using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Data.Abstracts.Common;
using Muhasebe.Data.Abstracts.Sistem;
using Muhasebe.Data.Abstracts.Sistem.Authentication;
using Muhasebe.Data.Concrete.Authentications;
using Muhasebe.Data.Concrete.Common;
using Muhasebe.Data.Concrete.EfRepositories.Sistem;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.DegerlerEntity;
using Muhasebe.Domain.Entities.SistemEntity;

namespace Muhasebe.Data.HostBuilders
{
    public static class AddRepositoryHostBuilderExtensions
    {
        public static IHostBuilder AddRepositories(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                services.AddScoped<IUnitOfWork<AppSistemDbContext>, UnitOfWork<AppSistemDbContext>>();
                services.AddScoped<IUnitOfWork<AppDbContext>, UnitOfWork<AppDbContext>>();
                services.AddScoped<IPasswordHasher<Kullanici>>(provider => new PasswordHasher<Kullanici>());

                //Repository
                services.AddScoped<IGenericRepository<SistemLog>, GenericRepository<SistemLog>>();
                services.AddScoped<IGenericRepository<AppLog>, GenericRepository<AppLog>>();
                services.AddScoped<IGenericRepository<Firma>, GenericRepository<Firma>>();
                services.AddScoped<IGenericRepository<Kullanici>, GenericRepository<Kullanici>>();

                services.AddScoped<ISistemLogRepository, SistemLogRepository>();
                services.AddScoped<IAppLogRepository, AppLogRepository>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
                services.AddScoped<IAccountStore, AccountStore>();
                services.AddScoped<IAuthenticator, Authenticator>();
                services.AddScoped<IFirmaRepository, FirmaRepository>();


            });
            return host;
        }
    }
}
