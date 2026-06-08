using HtmlGamer.Core.Data;
using HtmlGamer.Core.Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Runtime;
using System.Xml.Linq;
namespace HtmlGamer.Core.Services;
public sealed class Scrapper
{
    private readonly Random _random = new();

    private readonly ILogger<Scrapper> _logger;
    private readonly Writer _writter;

    private readonly string _path;
    private readonly string _gameUrl;
    private readonly string _masterPageSelector;

    private readonly string _masterUser;
    private readonly string _masterMail;
    private readonly string _masterPass;

    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    public Scrapper(ILogger<Scrapper> logger, Writer writter, AppSettings settings, IReadOnlyDictionary<string, string> config)
    {
        _logger = logger;
        _writter = writter;
        
        _path = Path.Combine(settings.Folders.Data, settings.Folders.ToParse);
        _gameUrl = config[Constants.EncryptKeys.MasterUrl];
        _masterPageSelector = config[Constants.EncryptKeys.MasterPageSelector];
        _masterUser = config[Constants.EncryptKeys.MasterUser];
        _masterMail = config[Constants.EncryptKeys.MasterMail];
        _masterPass = config[Constants.EncryptKeys.MasterPass];

        Loggers.LogAs.Init(_logger);
    }
    internal async Task ScrapBattleField(int? start = 1, int? end = 5)
    {
        int pageIndex = start!.Value;

        // SetUp Playwright and Browser
        using var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(Constants.Browsers.LaunchOptions);
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
        
        Loggers.Services.Scrapper.OpenBrowser(_logger, _browser.BrowserType.Name);
    
        //Load Page and Login
        await GoToPage(_gameUrl);
        await ClickButton("Log in!");
        await Login();
        
        //Navigate to starting point
        await ClickButtonOnImage("Attack");
        await GoToPage(pageIndex);
    
        while (true && pageIndex <= end!.Value)
        {
            Loggers.Services.Scrapper.ProcessPage(_logger, pageIndex, _page.Url);

            Uri uri = new(_page.Url);
            string pageName = Path.GetFileNameWithoutExtension(uri.AbsolutePath);

            await RandomDelay(500, 1000);
            await SaveCurrentPageHtml(_path, pageName, pageIndex);
       
            bool moved = await MoveToNextPage();
            if (!moved)
            {
                Loggers.LogAs.Debug(_logger, "No more pages.");
                break;
            }

            pageIndex++;

            await RandomDelay(3000, 7000);
        }

        Loggers.LogAs.Debug(_logger, "Done.");
    }
    internal async Task GoToPage(int pageId)
    {
        if (_page == null)
            return;

        await _page.Locator("input[name=\"page\"]").ClickAsync();
        await _page.Locator("input[name=\"page\"]").FillAsync(pageId.ToString());
        await _page.GetByRole(AriaRole.Cell, new() { Name = $"{pageId} Go", Exact = true }).Locator("input[type=\"submit\"]").ClickAsync();
        // await PageToLoad();

        Loggers.Services.Scrapper.GoToPage(_logger, _page.Url, pageId);
    }
    private async Task GoToPage(string url)
    {
        if (_page == null)
            return;

        Loggers.Services.Scrapper.GoToPage(_logger, url);

        await _page.GotoAsync(url);
        await PageToLoad();     
    }
    private async Task Login()
    {
        if (_page == null)
            return;

        Loggers.LogAs.Debug(_logger, "Logging in...");
      
        await FillField("Username", _masterUser);
        await FillField("Email", _masterMail);
        await FillField("Password", _masterPass);
        
        // MANUAL LOGIN STEP
        Console.WriteLine("Solve captcha manualy, then press ENTER...");
        Console.ReadLine();

        await ClickButton("Login");
    }
    private async Task PageToLoad()
    {
        if (_page == null)
            return;
        
        Loggers.LogAs.Debug(_logger, "Waiting page to load ...");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    private async Task FillField(string name, string value)
    {
        if (_page == null)
            return;

        Loggers.Services.Scrapper.FillField(_logger, name, value);
        await _page.GetByRole(AriaRole.Textbox, new() { Name = name }).ClickAsync();
        await _page.GetByRole(AriaRole.Textbox, new() { Name = name }).FillAsync(value);
    }
    private async Task ClickButtonOnImage(string name)
    {
        if (_page == null)
            return;

        Loggers.Services.Scrapper.ClickButton(_logger, name);
        name = name.ToLower();
        string? href = await _page
        .Locator($"area[alt='{name}']")
        .GetAttributeAsync("href");

        await GoToPage(new Uri(new Uri(_page.Url), href!).ToString());
    }
    private async Task ClickButton(string name)
    {
        if (_page == null)
            return;

        Loggers.Services.Scrapper.ClickButton(_logger, name);
        await _page.GetByRole(AriaRole.Button, new() { Name = name }).ClickAsync();
        
        await PageToLoad();
    } 
    internal async Task CheckPlayWrightInstallation()
    {
        try
        {
            using var playwright = await Playwright.CreateAsync();
            await playwright.Chromium.LaunchAsync();
        }
        catch (PlaywrightException ex)
            when (ex.Message.Contains("Executable doesn't exist"))
        {
            Loggers.LogAs.Error(_logger, "Playwright is not properly installed. Installing now...", ex);
            Program.Main(new[] { "install", "chromium" });
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, "Playwright is not properly installed. Installing now...", ex);
            Program.Main(new[] { "install", "chromium" });
        }
    }
    internal async Task SaveCurrentPageHtml(string path, string pageName, int pageIndex)
    {
        if (_page == null)
            return;

        string html = await _page.ContentAsync();
        string name = $"{pageName}_{pageIndex}";
        
        await _writter.SaveAsHtml(path, name, html);
    }
    internal async Task<bool> MoveToNextPage()
    {
        if (_page == null)
            return false;

        await _page.Locator("a")
            .Filter(new() { HasText = "Next" })
            .Last
            .ClickAsync();

        await PageToLoad();

        Loggers.LogAs.Debug(_logger, "Moving to next page...");
        return true;
    }
    internal async Task RandomDelay(int minMs = 1200, int maxMs = 4500)
    {
        int delay = _random.Next(minMs, maxMs);        
        Loggers.Services.Scrapper.RandomDelay(_logger, delay);
        await Task.Delay(delay);
    }
}
