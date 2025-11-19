namespace Shared.Response
{

    public enum ServiceErrorLevel
    {
        VALIDATION_ERROR,
        VALIDATION_WARNING,
        EXCEPTION
    }

    public class ServiceError
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; }
        public ServiceErrorLevel ErrorLevel { get; set; }

        public ServiceError() { }

        public ServiceError(Exception ex)
        {
            var finalException = ex;

            if (ex.InnerException != null)
                finalException = ex.InnerException;

            ErrorMessage = finalException.Message;
            ErrorDetail = finalException.ToString();
            ErrorLevel = ServiceErrorLevel.EXCEPTION;
        }

        public ServiceError(string errorCode)
        {
            ErrorCode = errorCode;
            ErrorLevel = ServiceErrorLevel.VALIDATION_ERROR;
        }

        public ServiceError(string errorCode, string errorMessage) : this(errorCode)
        {
            ErrorMessage = errorMessage;
        }

        public ServiceError(string errorCode, string errorMessage, ServiceErrorLevel errorLevel) : this(errorCode, errorMessage)
        {
            ErrorLevel = errorLevel;
        }

        public ServiceError(string errorCode, string errorMessage, string errorDetail) : this(errorCode, errorMessage)
        {
            ErrorDetail = errorDetail;
        }

        public ServiceError(string errorCode, string errorMessage, string errorDetail, ServiceErrorLevel errorLevel) : this(errorCode, errorMessage, errorDetail)
        {
            ErrorLevel = errorLevel;
        }
    }
}