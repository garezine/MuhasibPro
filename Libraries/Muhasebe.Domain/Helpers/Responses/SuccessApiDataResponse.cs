using Muhasebe.Domain.Enum;

namespace Muhasebe.Domain.Helpers.Responses
{
    public class SuccessApiDataResponse<T> : ApiDataResponse<T>
    {
        public SuccessApiDataResponse(T data, string message, bool success = true, ResultCodes resultCodes = ResultCodes.BASARILI_Tamamlandi, int resultCount = 0) : base(data, message, success, resultCodes, resultCount)
        {
        }
    }
}
