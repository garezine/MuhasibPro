using Muhasib.Domain.Enum;

namespace Muhasib.Data.Utilities.Responses
{
    public class SuccessApiResponse : ApiResponse
    {
        public SuccessApiResponse(string message, bool success = true, ResultCodes resultCodes = ResultCodes.BASARILI_Tamamlandi, int resultCount = 0) : base(message, success, resultCodes, resultCount)
        {
        }
    }
}
