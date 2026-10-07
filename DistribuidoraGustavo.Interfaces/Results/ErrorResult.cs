namespace DistribuidoraGustavo.Interfaces.Results;

public class ErrorResult
{
    public string ErrorMessage { get; set; } = string.Empty;
    public string FullException { get; set; } = string.Empty;
    public string FriendlyErrorMessage { get; set; } = string.Empty;

    public static ErrorResult Build(Exception ex)
        => new()
        {
            ErrorMessage = ex.Message,
            FullException = ex.ToString(),
            FriendlyErrorMessage = ex.Message
        };

    public static ErrorResult Build(string error)
        => new()
        {
            ErrorMessage = error,
            FullException = error,
            FriendlyErrorMessage = error
        };
}
