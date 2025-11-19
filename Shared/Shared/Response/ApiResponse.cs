using System.Net;

namespace Shared.Response
{
    public class ApiResponse<T>
    {
        public HttpStatusCode Code { get; set; }
        public List<ApiError> Errors { get; set; }
        public T Data { get; set; }
        public bool Status => !Errors.Any();
        public int ReturnValue { get; set; }
        public bool AsyncOperation { get; set; }

        protected ApiResponse()
        {
            Errors = new List<ApiError>();
        }

        public ApiResponse(ServiceResponse sr)
        {
            Errors = sr.Errors.Select(x => new ApiError(x)).ToList();
            Data = (T)sr.Data;
            Code = sr.Status ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
            ReturnValue = sr.ReturnValue;
            AsyncOperation = sr.AsyncOperation;
        }

        public ApiResponse(ServiceResponse<T> sr)
        {
            Errors = sr.Errors.Select(x => new ApiError(x)).ToList();
            Data = sr.Data;
            Code = sr.Status ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
            ReturnValue = sr.ReturnValue;
            AsyncOperation = sr.AsyncOperation;
        }

        public ApiResponse ToGeneric()
        {
            return new ApiResponse
            {
                Code = Code,
                Errors = Errors,
                Data = Data,
                ReturnValue = ReturnValue,
                AsyncOperation = AsyncOperation,
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse()
        {

        }

        public ApiResponse(ServiceResponse sr) : base(sr)
        {

        }
    }
}