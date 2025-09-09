namespace Nuggets.Application.Common;

public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool ok, T? value, string? error)
    {
        IsSuccess = ok;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(ok: true, value: value, error: null);
    public static Result<T> Err(string error) => new(ok: false, value: default, error: error);
}
