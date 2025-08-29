using Muhasebe.Data.Abstract.Common;
using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.Uygulama;
using Muhasebe.Domain.Interfaces.App;

namespace Muhasebe.Data.EfRepositories.App
{
    public class DefaultDataListRepository : IDefaultDataListRepository
    {
        private readonly IUnitOfWork<AppDbContext> _unitOfWork;

        public DefaultDataListRepository(IUnitOfWork<AppDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<Iller>> GetIllerAsync()
        {
            return await _unitOfWork.GetRepository<IGenericRepository<Iller>>().GetAllAsync();
        }
    }
}
