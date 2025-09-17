namespace Nuggets.Application.Common;

public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(bool ok, T? value, string? error, string? errorCode = null)
    {
        IsSuccess = ok;
        Value = value;
        Error = error;
        ErrorCode = errorCode;
    }
    
    public static Result<T> Ok(T value) => new(true, value, null);

    public static Result<T> Err(string error, string errorCode = "GENERIC_ERROR")
        => new(false, default, error, errorCode);
}
