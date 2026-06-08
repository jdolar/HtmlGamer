using HtmlGamer.Core.Data;
using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.InPut;
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
    private static Scenario[] GetScenarios(Encryption encryption, Reader reader)
    {
        string json = reader.GetJson(Path.Combine(_directory, Constants.Folders.Config), Constants.Files.Scenarios);
        
        var scenarios = JsonSerializer.Deserialize<Scenario[]>(json, Constants.Json.SerializerOptions) ?? new Scenario[0];
        
        return scenarios;
    }
    private static Account[] GetAccounts(Encryption encryption, Reader reader)
    {
        string json = reader.GetJson(Path.Combine(_directory, Constants.Folders.Config), Constants.Files.Accounts);

        var accounts = JsonSerializer.Deserialize<Account[]>(json, Constants.Json.SerializerOptions) ?? new Account[0];

        return accounts;
    }
    private static Data.Models.InPut.Execute[] GetExecutes(Encryption encryption, Reader reader)
    {
        string json = reader.GetJson(Path.Combine(_directory, Constants.Folders.Config), Constants.Files.Execute);

        var executes = JsonSerializer.Deserialize<Data.Models.InPut.Execute[]>(json, Constants.Json.SerializerOptions) ?? new Data.Models.InPut.Execute[0];

        return executes;
    }
    public static IHost BuildHost(string[] args, bool? encryptConstants = null)
    {
        Encryption encrypt = new();
        Reader reader = new();
        Writer writter = new();

        if (encryptConstants == true)
        {
            Dictionary<string, string> encrypted = EncryptConstansts(encrypt, reader);
            writter.SaveAsDictionary(_directory, Constants.Files.Encrypted, encrypted);
        }

        return Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();
            config.AddJsonFile($"{Constants.Folders.Config}/{Constants.Files.AppSettings}", optional: false, reloadOnChange: true)
                  .AddJsonFile($"{Constants.Folders.Config}/{Constants.Files.AppSettings}.{context.HostingEnvironment.EnvironmentName}.json",
                                     optional: true, reloadOnChange: true);

            config.AddEnvironmentVariables();
             })
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

            if (string.IsNullOrWhiteSpace(settings.Folders.ToParse))
                settings.Folders.ToParse = Path.Combine(settings.Folders.Data, settings.Folders.ToParse);

            if (string.IsNullOrWhiteSpace(settings.Folders.Parsed))
                settings.Folders.Parsed = Path.Combine(settings.Folders.Data, settings.Folders.Parsed);

            if (string.IsNullOrWhiteSpace(settings.Folders.Analyzed))
                settings.Folders.Analyzed = Path.Combine(settings.Folders.Data, settings.Folders.Analyzed);

            services.AddSingleton(settings);

            var scnarios = GetScenarios(encrypt, reader);
            services.AddSingleton<IReadOnlyList<Scenario>>(scnarios);

            var accounts = GetAccounts(encrypt, reader);
            services.AddSingleton<IReadOnlyList<Account>>(accounts);

            var executes = GetExecutes(encrypt, reader);
            services.AddSingleton<IReadOnlyList<Data.Models.InPut.Execute>>(executes);

            Dictionary<string, string> config = DecryptConstansts(encrypt, reader);
            services.AddSingleton<IReadOnlyDictionary<string, string>>(config);

            services.AddSingleton<Encryption>();
            services.AddSingleton<Reader>();
            services.AddSingleton<Writer>();
            services.AddSingleton<Mapper>();
            services.AddSingleton<Parser>();
            services.AddSingleton<Scrapper>();
            services.AddSingleton<Runner>();
        })
        .Build();
    }
}