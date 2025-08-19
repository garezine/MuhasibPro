using Microsoft.EntityFrameworkCore;
using Muhasebe.Data.DataContext;
using Muhasebe.Data.EfRepositories.Common;
using Muhasebe.Domain.Entities.SistemDb;
using Muhasebe.Domain.Interfaces.App.IAuthentication;

namespace Muhasebe.Data.EfRepositories.App.Authentications
{
    public class UserRepository : GenericRepository<Kullanici>, IUserRepository
    {
        public UserRepository(AppSistemDbContext context) : base(context)
        {
        }
        public async Task<Kullanici> GetByEmailAsync(string email)
        {
            if(email == null) 
                throw new ArgumentNullException("email");
            var entity = await _context.Set<Kullanici>()
                .FirstOrDefaultAsync((e) => e.Eposta == email)
                .ConfigureAwait(false);
            return entity;
        }

        public async Task<Kullanici> GetByUsernameAsync(string userName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");
            var entity = await _context.Set<Kullanici>()
                .FirstOrDefaultAsync((e) => e.KullaniciAdi == userName)
                .ConfigureAwait(false);
            return entity;
        }
    }
}
