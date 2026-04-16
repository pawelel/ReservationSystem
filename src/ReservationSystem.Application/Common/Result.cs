namespace ReservationSystem.Application.Common;

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Forbidden,
    Conflict
}

public readonly record struct Result<T>(bool IsSuccess, T? Value, string? Error, ErrorType ErrorType)
{
    public static Result<T> Success(T value) => new(true, value, null, ErrorType.None);
    public static Result<T> Validation(string msg) => new(false, default, msg, ErrorType.Validation);
    public static Result<T> NotFound(string msg) => new(false, default, msg, ErrorType.NotFound);
    public static Result<T> Forbidden(string msg) => new(false, default, msg, ErrorType.Forbidden);
    public static Result<T> Conflict(string msg) => new(false, default, msg, ErrorType.Conflict);
}

public readonly record struct Result(bool IsSuccess, string? Error, ErrorType ErrorType)
{
    public static Result Success() => new(true, null, ErrorType.None);
    public static Result Validation(string msg) => new(false, msg, ErrorType.Validation);
    public static Result NotFound(string msg) => new(false, msg, ErrorType.NotFound);
    public static Result Forbidden(string msg) => new(false, msg, ErrorType.Forbidden);
    public static Result Conflict(string msg) => new(false, msg, ErrorType.Conflict);
}
