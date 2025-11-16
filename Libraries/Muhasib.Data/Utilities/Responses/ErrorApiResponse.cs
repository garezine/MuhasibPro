using Muhasib.Domain.Enum;

namespace Muhasib.Data.Utilities.Responses
{
    public class ErrorApiResponse : ApiResponse
    {
        public ErrorApiResponse(string message, bool success = false, ResultCodes resultCodes = ResultCodes.HATA_BilinmeyenHata, int resultCount = 0) : base(message, success, resultCodes, resultCount)
        {
        }
    }
}
