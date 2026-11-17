namespace eRynok.Services
{
    public enum ServiceError
    {
        None,
        NotFound,
        InvalidReference,
        Conflict
    }

    public class ServiceResult<T>
    {
        public bool IsSuccess { get; private set; }

        public T? Data { get; private set; }

        public ServiceError Error { get; private set; }

        public string? Message { get; private set; }

        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                Error = ServiceError.None
            };
        }

        public static ServiceResult<T> Failure(
            ServiceError error,
            string message)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Error = error,
                Message = message
            };
        }
    }
}
