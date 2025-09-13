using Muhasebe.Data.Abstracts.Common;
using Muhasebe.Domain.Entities.SistemEntity;

namespace Muhasebe.Data.Abstracts.Sistem.Authentication
{
    public interface IUserRepository : IGenericRepository<Kullanici>
    {
        Task<Kullanici> GetByUsernameAsync(string userName);
        Task<Kullanici> GetByEmailAsync(string email);
    }
}
