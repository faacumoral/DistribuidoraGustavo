using DistribuidoraGustavo.Interfaces.Settings;

namespace DistribuidoraGustavo.API
{
    public static class Configure
    {
        
        public static void AddConfig(IConfiguration configuration)
        {
            ApplicationSettings.Init(new ApplicationSettings
            {
                EncryptKey = configuration["EncryptKey"]
            });
        }
    }
}
