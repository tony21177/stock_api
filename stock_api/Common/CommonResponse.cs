

using FluentValidation.Results;

namespace stock_api.Common
{
    public class CommonResponse<T>
    {
        public bool Result { get; set; } = false;
        public string Message { get; set; } = "";

        public T? Data { get; set; }
        public int? TotalPages { get; set; } = 0;
        public static CommonResponse<dynamic> BuildNotAuthorizeResponse()
        {
            return new CommonResponse<dynamic>
            {
                Result = false,
                Message = "沒有權限",
            };
        }

        public static CommonResponse<dynamic> BuildValidationFailedResponse(ValidationResult validationResult)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);

            return new CommonResponse<dynamic>
            {
                Result = false,
                Message = string.Join(", ", errors),
            };
        }
    }
}
