using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.Mapping;
using Muhasebe.Data.DataContext;
using Muhasebe.Data.EfRepositories.App;
using Muhasebe.Data.EfRepositories.App.Authentications;
using Muhasebe.Data.EfRepositories.Common;
using Muhasebe.Domain.Interfaces.App;
using Muhasebe.Domain.Interfaces.App.IAuthentication;
using Muhasebe.Domain.Interfaces.App.IFirma;
using Muhasebe.Domain.Interfaces.App.IStore;
using Muhasebe.Domain.Interfaces.Database;

namespace Muhasebe.Business.HostBuilders
{
    public static class AddRepositoryHostBuilderExtensions
    {
        public static IHostBuilder AddRepository(this IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                services.AddAutoMapper(cfg => cfg.AddProfile<GenelMap>());
                services.AddScoped<IMapper, Mapper>();
                services.AddScoped<IUnitOfWork<AppSistemDbContext>, UnitOfWork<AppSistemDbContext>>();
                services.AddScoped<IUnitOfWork<AppDbContext>, UnitOfWork<AppDbContext>>();


                //Repository
                services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

                services.AddScoped<ILogRepository, LogRepository>();
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
