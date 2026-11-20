namespace soft.square.net.WebApi.Modules.ModuleName.UseCases;

public static class ResultExtensions
{
    /// <summary>A failed <see cref="Result"/> as a <see cref="Result{T}"/>: same status, same messages.</summary>
    public static Result<T> As<T>(this Result failure) => failure.Status switch
    {
        ResultStatus.Forbidden => Result<T>.Forbidden([.. failure.Errors]),
        ResultStatus.NotFound => Result<T>.NotFound([.. failure.Errors]),
        ResultStatus.Conflict => Result<T>.Conflict([.. failure.Errors]),
        ResultStatus.Unauthorized => Result<T>.Unauthorized([.. failure.Errors]),
        ResultStatus.Invalid => Result<T>.Invalid([.. failure.ValidationErrors]),
        ResultStatus.Ok => throw new InvalidOperationException("Only a failed result converts."),
        _ => Result<T>.Error(new ErrorList([.. failure.Errors]))
    };
}
