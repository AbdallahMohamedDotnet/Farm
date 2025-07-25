using System.Security.Cryptography;
using System.Text;

namespace WebApplication2.Services
{
    public class TokenEncryptionService : ITokenEncryptionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenEncryptionService> _logger;
        private readonly string _encryptionKey;
        private readonly string _iv;

        public TokenEncryptionService(IConfiguration configuration, ILogger<TokenEncryptionService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _encryptionKey = _configuration["SecuritySettings:EncryptionKey"] ?? GenerateSecureKey();
            _iv = _configuration["SecuritySettings:IV"] ?? GenerateIV();
        }

        public string EncryptToken(string token)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Convert.FromBase64String(_encryptionKey);
                aes.IV = Convert.FromBase64String(_iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using var swEncrypt = new StreamWriter(csEncrypt);

                swEncrypt.Write(token);
                swEncrypt.Close();
                
                var encrypted = msEncrypt.ToArray();
                var result = Convert.ToBase64String(encrypted);
                
                _logger.LogInformation("Token encrypted successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt token");
                throw new InvalidOperationException("Token encryption failed", ex);
            }
        }

        public string DecryptToken(string encryptedToken)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Convert.FromBase64String(_encryptionKey);
                aes.IV = Convert.FromBase64String(_iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedToken));
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                var result = srDecrypt.ReadToEnd();
                _logger.LogInformation("Token decrypted successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt token");
                throw new InvalidOperationException("Token decryption failed", ex);
            }
        }

        public string GenerateSecureKey()
        {
            using var aes = Aes.Create();
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }

        private string GenerateIV()
        {
            using var aes = Aes.Create();
            aes.GenerateIV();
            return Convert.ToBase64String(aes.IV);
        }

        public bool ValidateTokenIntegrity(string token)
        {
            try
            {
                // Validate token format and structure
                if (string.IsNullOrWhiteSpace(token))
                    return false;

                // Check if token has proper JWT structure
                var parts = token.Split('.');
                if (parts.Length != 3)
                    return false;

                // Validate each part is valid Base64
                foreach (var part in parts)
                {
                    try
                    {
                        Convert.FromBase64String(AddPadding(part));
                    }
                    catch
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token integrity validation failed");
                return false;
            }
        }

        private static string AddPadding(string base64)
        {
            var padding = 4 - base64.Length % 4;
            if (padding != 4)
            {
                base64 += new string('=', padding);
            }
            return base64;
        }
    }
}