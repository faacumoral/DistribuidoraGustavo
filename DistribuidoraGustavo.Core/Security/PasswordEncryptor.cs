using System.Security.Cryptography;
using System.Text;

namespace DistribuidoraGustavo.Core.Security;

public static class PasswordEncryptor
{
    // Estos valores definen el formato de las contraseñas ya guardadas en Users.Password:
    // cambiar cualquiera invalida todas las contraseñas existentes.
    private const string SALT = "aseweias38490a76";
    private const string VECTOR = "8947az44a5wlkwyt";
    private const string HASH = "SHA1";
    private const int ITERATIONS = 2;
    private const int KEY_SIZE = 256;

    public static string Encrypt(string value, string password)
    {
        var saltBytes = Encoding.ASCII.GetBytes(SALT);
        var vectorBytes = Encoding.ASCII.GetBytes(VECTOR);
        var valueBytes = Encoding.UTF8.GetBytes(value);

        using var passwordBytes = new PasswordDeriveBytes(password, saltBytes, HASH, ITERATIONS);
        var keyBytes = passwordBytes.GetBytes(KEY_SIZE / 8);

        using var aes = Aes.Create();
        aes.Key = keyBytes;

        var encrypted = aes.EncryptCbc(valueBytes, vectorBytes);
        return Convert.ToBase64String(encrypted);
    }
}
