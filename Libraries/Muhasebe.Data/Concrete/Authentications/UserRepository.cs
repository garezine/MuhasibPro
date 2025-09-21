using Muhasebe.Data.Abstracts.Common;
using Muhasebe.Data.Abstracts.Sistem.Authentication;
using Muhasebe.Data.Concrete.Common;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.SistemEntity;

namespace Muhasebe.Data.Concrete.Authentications
{
    public class UserRepository : GenericRepository<Kullanici>, IUserRepository
    {
        private readonly IUnitOfWork<AppSistemDbContext> _unitOfWork;
        private readonly AppSistemDbContext _dbContext;
        public UserRepository(IUnitOfWork<AppSistemDbContext> unitOfWork) : base(unitOfWork.Context)
        {
            _unitOfWork = unitOfWork;
            _dbContext = unitOfWork.Context ?? throw new ArgumentNullException(nameof(unitOfWork.Context));
        }
        public async Task<Kullanici> GetByEmailAsync(string email)
        {
            if (email == null)
                throw new ArgumentNullException("email");
            var entity = await base.FirstOrDefaultAsync(e => e.Eposta == email)
                .ConfigureAwait(false);
            return entity;
        }

        public async Task<Kullanici> GetByUsernameAsync(string userName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");
            var entity = await base.FirstOrDefaultAsync((e) => e.KullaniciAdi == userName)
                .ConfigureAwait(false);
            return entity;
        }
    }
}
