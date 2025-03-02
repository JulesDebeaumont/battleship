namespace Server.Services.Utils;

public class ServiceError : Exception
{
    public EServiceErrorType ServiceErrorType { get; set; }
    public Exception? Exception { get; set; }

    public ServiceError(string message, EServiceErrorType errorType, Exception exception) : base(message)
    {
        ServiceErrorType = errorType;
        Exception = exception;
    }
    
    public ServiceError(string message, EServiceErrorType errorType) : base(message)
    {
        ServiceErrorType = errorType;
    }
    
    public enum EServiceErrorType
    {
        NotFound,
        NotAuthorized,
        NotAuthenticated,
        InternalError
    }
}
