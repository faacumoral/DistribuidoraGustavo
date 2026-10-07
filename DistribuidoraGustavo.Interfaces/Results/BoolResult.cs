namespace DistribuidoraGustavo.Interfaces.Results;

public class BoolResult : BaseResult<bool>
{
    public static BoolResult Ok() => BuildOk<BoolResult>(true);

    public static BoolResult Error(Exception ex) => BuildError<BoolResult>(ErrorResult.Build(ex));

    public static BoolResult Error(string error) => BuildError<BoolResult>(ErrorResult.Build(error));
}
