using System.Security.Cryptography;
using System.Text;

namespace BrazaImoveis.Infrastructure.ApiKey;
public class ApiKeyGenerator
{
    public static string GenerateApiKey(int keyLength)
    {
        const string characterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";

        byte[] randomBytes = RandomNumberGenerator.GetBytes(keyLength);

        StringBuilder apiKeyBuilder = new StringBuilder(keyLength);
        foreach (byte randomByte in randomBytes)
        {
            apiKeyBuilder.Append(characterSet[randomByte % characterSet.Length]);
        }

        string apiKey = apiKeyBuilder.ToString();

        // Optional: Hash the API key
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
            apiKey = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }

        return apiKey;
    }
}
