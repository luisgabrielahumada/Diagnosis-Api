namespace Shared.Response
{
    public class ApiError
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; }
        public ServiceErrorLevel ErrorLevel { get; set; }

        public ApiError(ServiceError x)
        {
            ErrorCode = x.ErrorCode;
            ErrorMessage = x.ErrorMessage;
            ErrorDetail = x.ErrorDetail;
            ErrorLevel = x.ErrorLevel;
        }
    }
}
