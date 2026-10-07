namespace DistribuidoraGustavo.Interfaces.Results;

public class StringResult : BaseResult<string>
{
    public static StringResult Ok(string ok) => BuildOk<StringResult>(ok);

    public static StringResult Error(Exception ex) => BuildError<StringResult>(ErrorResult.Build(ex));

    public static StringResult Error(string error) => BuildError<StringResult>(ErrorResult.Build(error));
}
