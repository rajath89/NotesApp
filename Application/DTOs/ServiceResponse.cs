namespace Application.DTOs;

public class ServiceResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }

    public static ServiceResponse<T> SuccessResponse(T data)
    {
        return new ServiceResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ServiceResponse<T> FailureResponse(string errorMessage, string errorCode = "SYSTEMDOWN")
    {
        return new ServiceResponse<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}