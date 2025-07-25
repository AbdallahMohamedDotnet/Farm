using System.Security.Cryptography;
using System.Text;

namespace WebApplication2.Services
{
    public interface ITokenEncryptionService
    {
        string EncryptToken(string token);
        string DecryptToken(string encryptedToken);
        string GenerateSecureKey();
        bool ValidateTokenIntegrity(string token);
    }
}