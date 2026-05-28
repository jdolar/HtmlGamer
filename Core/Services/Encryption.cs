using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Playwright;
using System.Security.Cryptography;
using System.Text;
namespace HtmlGamer.Core.Services;
public sealed class Encryption
{
    internal readonly ILogger<Encryption> _logger;
    internal readonly byte[] _key;
    internal readonly byte[] _iv;
    public Encryption()
        : this(NullLogger<Encryption>.Instance)
    {
    }
    public Encryption(ILogger<Encryption> logger)
    {
        _logger = logger;
        Loggers.LogAs.Init(_logger);
        
        string seed = typeof(Program).Assembly.FullName!;
        _key = SHA256.HashData(
            Encoding.UTF8.GetBytes(seed));

        _iv = SHA256.HashData(
                Encoding.UTF8.GetBytes(seed + "-iv"))
            .Take(16)
            .ToArray();
    }
    internal string Encrypt(string input)
    {
        try
        {
            Loggers.LogAs.Debug(_logger, "Encrypting input: {input}");

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using MemoryStream msEncrypt = new();

            using (CryptoStream csEncrypt =
                   new(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (StreamWriter swEncrypt = new(csEncrypt))
            {
                swEncrypt.Write(input);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, "Failed to encrypt input:{input}", ex);
            return string.Empty;
        }
    }
    internal string Decrypt(string input)
    {
        try
        {
            Loggers.LogAs.Debug(_logger, "Decrypting input: {input}");

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            using MemoryStream msDecrypt = new(Convert.FromBase64String(input));
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, "Failed to decrypt input: {input}", ex);
            return string.Empty;
        }
    }
    internal Dictionary<string, string> DecryptConstants(Dictionary<string, string> content)
    {
        try
        {
            var decrypted = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kvp in content)
                decrypted[kvp.Key] = Decrypt(kvp.Value);

            return decrypted;
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, "Failed to decrypt dictionary", ex);
            return new Dictionary<string, string>();
        }
    }
    internal Dictionary<string, string> EncryptConstants(Dictionary<string, string> content)
    {
        try
        {
            var encryptedContent = new Dictionary<string, string>();
            
            foreach (KeyValuePair<string, string> kvp in content)
                encryptedContent[kvp.Key] = Encrypt(kvp.Value);

            return encryptedContent;
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, "Failed to encrypt dictionary", ex);
            return new Dictionary<string, string>();
        }
    }
}
