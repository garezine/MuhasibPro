using System.Linq.Expressions;

namespace Muhasebe.Domain.Helpers
{
    public class DataRequest<T>
    {
        public string Query { get; set; }
        public Expression<Func<T, bool>> Where { get; set; }
        public Expression<Func<T, object>> OrderBy { get; set; }
        public Expression<Func<T, object>> OrderByDesc { get; set; }
        public Expression<Func<T, object>>[] Includes { get; set; }
    }
}
