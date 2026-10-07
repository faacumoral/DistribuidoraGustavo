namespace DistribuidoraGustavo.Interfaces.Settings;

public class JwtConfiguration
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string SecretKey { get; set; }
    public int ExpireMinutes { get; set; }
}
