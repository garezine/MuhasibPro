using Muhasib.Domain.Enum;

namespace Muhasib.Data.Utilities.Responses
{
    public class ApiDataResponse<T> : ApiResponse
    {
        public ApiDataResponse(T data, string message, bool success, ResultCodes resultCodes, int resultCount) : base(message, success, resultCodes, resultCount)
        {
            Data = data;
        }

        public T Data { get; set; }

    }
}
