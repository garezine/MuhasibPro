using Muhasebe.Data.DataContext;
using Muhasebe.Domain.Entities.Uygulama;
using Muhasebe.Domain.Interfaces.App;
using Muhasebe.Domain.Interfaces.Database;

namespace Muhasebe.Data.EfRepositories.App
{
    public class EntityTablesListRepository : IEntityTablesListRepository
    {
        private readonly IUnitOfWork<AppSistemDbContext> _unitOfWork;

        public EntityTablesListRepository(IUnitOfWork<AppSistemDbContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<Iller>> GetIllerAsync()
        {
            return await _unitOfWork.GetRepository<IGenericRepository<Iller>>().GetAllAsync();
        }
    }
}
