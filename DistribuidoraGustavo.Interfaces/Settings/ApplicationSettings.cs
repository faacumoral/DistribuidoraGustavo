namespace DistribuidoraGustavo.Interfaces.Settings;

public class ApplicationSettings
{
    #nullable disable
    public static ApplicationSettings Config { get; private set; } = null;

    public static void Init(ApplicationSettings configuration)
    {
        if (Config is not null) throw new NotSupportedException();

        Config = configuration;
    }

    public string EncryptKey { get; set; } = string.Empty;
}
