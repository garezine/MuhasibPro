using Microsoft.EntityFrameworkCore;
using Muhasib.Data.BaseRepositories;
using Muhasib.Data.Contracts.SistemRepositories;
using Muhasib.Data.DataContext;
using Muhasib.Domain.Entities.SistemEntity;

namespace Muhasib.Data.Repositories.SistemRepositories
{
    public class UserRepository : BaseRepository<SistemDbContext, Kullanici>, IUserRepository
    {
        public UserRepository(SistemDbContext context) : base(context)
        {
        }

        public async Task<Kullanici?> GetByEmailAsync(string email)
        {
            if (email == null)
                throw new ArgumentNullException("email");
            return await DbSet
                .FirstOrDefaultAsync(u => u.Eposta == email)
                .ConfigureAwait(false);
        }

        public async Task<Kullanici?> GetByUsernameAsync(string userName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");
            return await DbSet
                .Include(a=> a.Rol)
                .FirstOrDefaultAsync(u => u.KullaniciAdi == userName)                
                .ConfigureAwait(false);
        }
    }
}
