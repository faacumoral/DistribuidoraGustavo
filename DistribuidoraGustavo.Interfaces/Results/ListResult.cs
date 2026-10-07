namespace DistribuidoraGustavo.Interfaces.Results;

public class ListResult<T> : BaseResult<List<T>>
{
    public static ListResult<T> Ok(List<T> ok) => BuildOk<ListResult<T>>(ok);

    public static ListResult<T> Error(Exception ex) => BuildError<ListResult<T>>(ErrorResult.Build(ex));

    public static ListResult<T> Error(string error) => BuildError<ListResult<T>>(ErrorResult.Build(error));
}
