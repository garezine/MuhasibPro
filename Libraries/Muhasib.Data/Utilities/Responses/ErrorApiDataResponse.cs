using Muhasib.Domain.Enum;

namespace Muhasib.Data.Utilities.Responses
{
    public class ErrorApiDataResponse<T> : ApiDataResponse<T>
    {
        public ErrorApiDataResponse(T data, string message, bool success = false, ResultCodes resultCodes = ResultCodes.HATA_BilinmeyenHata, int resultCount = 0)
            : base(data, message, success, resultCodes, resultCount)
        {
        }
    }
}
