namespace Server.Services.Utils;

public abstract class ResponseServiceAbstract
{
    public bool IsSuccess { get; set; }
    public List<string> Errors { get; set; } = [];

    public void AddError(string error)
    {
        Errors.Add(error);
    }

    public bool EnsureOtherServiceIsSuccess(ResponseServiceAbstract anotherResponseService)
    {
        if (!anotherResponseService.IsSuccess)
        {
            Errors = anotherResponseService.Errors;
            return false;
        }
        return true;
    }
}


public class ResponseService : ResponseServiceAbstract
{
    public void SetSuccessful()
    {
        IsSuccess = true;
    }
}

public class ResponseService<T> : ResponseServiceAbstract
{
    public T Data { get; set; }

    public void SetSuccessful(T data)
    {
        Data = data;
        IsSuccess = true;
    }
}