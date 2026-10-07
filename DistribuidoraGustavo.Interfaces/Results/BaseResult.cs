namespace DistribuidoraGustavo.Interfaces.Results;

public abstract class BaseResult<T> : IBaseErrorResult
{
    public T ResultOk { get; set; }
    public ErrorResult ResultError { get; set; }
    public ResultOperation ResultOperation { get; set; } = ResultOperation.Ok;
    public bool Success { get; set; } = true;

    protected static TResult BuildOk<TResult>(T ok)
        where TResult : BaseResult<T>, new()
        => new()
        {
            ResultOk = ok,
            ResultOperation = ResultOperation.Ok,
            Success = true
        };

    protected static TResult BuildError<TResult>(ErrorResult error)
        where TResult : BaseResult<T>, new()
        => new()
        {
            ResultError = error,
            ResultOperation = ResultOperation.Error,
            Success = false
        };
}
