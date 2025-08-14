using Muhasebe.Business.Common;

namespace Muhasebe.Business.Services.Abstract.Common
{
    public interface IGenericService<TModel,T> where TModel : BaseModel
        where T : class
    {    
    }
}
