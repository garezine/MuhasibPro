using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Muhasebe.Business.Mapping;
using Muhasebe.Data.Abstract.Common;
using Muhasebe.Data.Abstract.Sistem;
using Muhasebe.Data.Abstract.Sistem.Authentication;
using Muhasebe.Data.DataContext;
using Muhasebe.Data.EfRepositories.App;
using Muhasebe.Data.EfRepositories.App.Authentications;
using Muhasebe.Data.EfRepositories.Common;
using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Entities.Uygulama;

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
                services.AddScoped<IGenericRepository<AppLog>,GenericRepository<AppLog>>();
                services.AddScoped<IGenericRepository<Firma>,GenericRepository<Firma>>();
                services.AddScoped<IGenericRepository<Kullanici>,GenericRepository<Kullanici>>();

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
