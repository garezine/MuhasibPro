﻿using Muhasebe.Domain.Enum;

namespace Muhasebe.Domain.Helpers.Responses
{
    public class ApiResponse
    {
        public ApiResponse(string message, bool success, ResultCodes resultCodes, int resultCount)
        {
            Message = message;
            Success = success;
            ResultCode = (int)resultCodes;
            ResultCount = resultCount;
        }

        public string Message { get; set; }
        public bool Success { get; set; }
        public int ResultCode { get; set; }
        public int ResultCount { get; set; }

    }
}
