namespace DistribuidoraGustavo.Interfaces.Results;

public class DTOResult<T> : BaseResult<T>
    where T : class
{
    public static DTOResult<T> Ok(T ok) => BuildOk<DTOResult<T>>(ok);

    public static DTOResult<T> Error(Exception ex) => BuildError<DTOResult<T>>(ErrorResult.Build(ex));

    public static DTOResult<T> Error(string error) => BuildError<DTOResult<T>>(ErrorResult.Build(error));

    public static implicit operator DTOResult<T>(T ok) => Ok(ok);
}
