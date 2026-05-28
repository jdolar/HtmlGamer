using HtmlGamer.Core.Data;
using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
namespace HtmlGamer.Core;

public sealed class Configure
{
    private static string _directory = Environment.CurrentDirectory;
    private static Dictionary<string, string> EncryptConstansts(Encryption encryption, Reader reader)
    {
        string decryptedJson = reader.GetJson("F:\\HtmlGamer", Constants.Files.Encrypt);

        Dictionary<string, string> decrypted = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedJson) ?? [];
        Dictionary<string, string> encrypted = encryption.EncryptConstants(decrypted);

        return encrypted;
    }
    private static Dictionary<string, string> DecryptConstansts(Encryption encryption, Reader reader)
    {
        string encryptedJson = reader.GetFile(_directory, Constants.Files.Encrypted);

        Dictionary<string, string> encrypted = JsonSerializer.Deserialize<Dictionary<string, string>>(encryptedJson) ?? [];
        Dictionary<string, string> decrypted = encryption.DecryptConstants(encrypted);

        return decrypted;
    }
    public static IHost BuildHost(string[] args, bool? encryptConstants = null)
    {
        Encryption encrypt = new();
        Reader reader = new();
        Writter writter = new();

        if (encryptConstants == true)
        {
            Dictionary<string, string> encrypted = EncryptConstansts(encrypt, reader);
            writter.SaveAsDictionary(_directory, Constants.Files.Encrypted, encrypted);
        }

        Dictionary<string, string> config = DecryptConstansts(encrypt, reader);

        return Host.CreateDefaultBuilder(args)
        .ConfigureLogging((context, logging) =>
        {
            logging.AddConfiguration(context.Configuration.GetSection("Logging"));
            logging.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "[HH:mm:ss] ";
            });
        })
        .ConfigureServices((context, services) =>
        {
            var settings = context.Configuration.Get<AppSettings>() ?? new();

            if (string.IsNullOrWhiteSpace(settings.Folders.Data))
                settings.Folders.Data = _directory;

            if (string.IsNullOrWhiteSpace(settings.Folders.InPut))
                settings.Folders.InPut = nameof(settings.Folders.InPut);

            if (string.IsNullOrWhiteSpace(settings.Folders.OutPut))
                settings.Folders.OutPut = nameof(settings.Folders.OutPut);

            if (string.IsNullOrWhiteSpace(settings.Folders.ToEncrypt))
                settings.Folders.ToEncrypt = nameof(settings.Folders.ToEncrypt);

            services.AddSingleton(settings);
            services.AddSingleton<IReadOnlyDictionary<string, string>>(config);

            services.AddSingleton<Encryption>();
            services.AddSingleton<Reader>();
            services.AddSingleton<Writter>();
            services.AddSingleton<Mapper>();
            services.AddSingleton<Parser>();
            services.AddSingleton<Scrapper>();
        })
        .Build();
    }
}