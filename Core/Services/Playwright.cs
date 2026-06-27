using HtmlGamer.Core.Data;
using HtmlGamer.Core.Data.Models;
using HtmlGamer.Core.Data.Models.InPut;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
namespace HtmlGamer.Core.Services;
public sealed class Playwright
{
    private readonly Random _random = new();

    private readonly ILogger<Playwright> _logger;
    private readonly Writer _writter;

    private readonly string _path;
    private readonly string _gameUrl;
    public Playwright(ILogger<Playwright> logger, Writer writter, AppSettings settings, IReadOnlyDictionary<string, string> config)
    {
        _logger = logger;
        _writter = writter;

        _path = Path.Combine(settings.Folders.Data, settings.Folders.ToParse);
        _gameUrl = config[Constants.EncryptKeys.MasterUrl];

        Loggers.LogAs.Init(_logger);
    }
    internal async Task Execute(Func<IPage, Task> scenario)
    {
        try
        {
            using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(Constants.Browsers.LaunchOptions);
            await using var context = await browser.NewContextAsync();

            Loggers.Services.Playwright.OpenBrowser(_logger, browser.BrowserType.Name);
            var page = await context.NewPageAsync();
            await scenario(page);
        }
        catch (Exception ex)
        {
            Loggers.LogAs.Error(_logger, "An error occurred while executing the action.", ex);
        }
    }

    #region Scenario Steps
    public async Task Login(IPage page, Account account, Scenario scenario)
    {
        if (page == null)
            return;

        Loggers.LogAs.Debug(_logger, "Logging in...");

        await GoToPageUrl(page, _gameUrl);
        await ClickButton(page, "Log in!");

        await FillField(page, "Username", account.UserName);
        await FillField(page, "Email", account.Email);
        await FillField(page, "Password", account.PassWord);

        // MANUAL LOGIN STEP
        Console.WriteLine("Solve captcha manualy, then press ENTER...");
        Console.ReadLine();

        await ClickButton(page, "Login");
    }
    public async Task Bank(IPage page, Account account, Scenario scenario)
    {
        await GoToPageUrl(page, new Uri(new Uri(page.Url), "bank.php"!).ToString());
        
        string naqOnHand = await page
                    .GetByRole(AriaRole.Textbox, new() { Name = "amount:" })
                    .InputValueAsync();
        Loggers.Services.Playwright.NaqOnHand(_logger, account.UserName, naqOnHand);
        
        await page.Locator("html").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Deposit" }).ClickAsync();
    }
    public async Task ScrapBattlefield(IPage page, Account account, Scenario scenario)
    {
        int pageStart = 1;
        int pageEnd = 1;

        if (scenario?.Properties?.TryGetValue(nameof(ScrapBattlefield), out List<Property>? properties) == true)
        {
            if (properties.FirstOrDefault(p => p.Name == Constants.ScenarioKeys.StartIndex)?.Value is string pageStartStr && int.TryParse(pageStartStr, out int start))
                pageStart = start;

            if (properties.FirstOrDefault(p => p.Name == Constants.ScenarioKeys.EndIndex)?.Value is string pageEndStr && int.TryParse(pageEndStr, out int end))
                pageEnd = end;
        }

        int pageIndex = pageStart;
        string pageIndexStr = pageIndex.ToString();

        //Navigate to starting point
        await ClickButtonOnImage(page, "Attack");
        await GoToPageIndex(page, pageIndexStr);

        while (true && pageIndex <= pageEnd)
        {
            Loggers.Services.Playwright.ProcessPage(_logger, pageIndexStr, page.Url);

            Uri uri = new(page.Url);
            string pageName = $"{account.UserName}_{Path.GetFileNameWithoutExtension(uri.AbsolutePath)}";       
            await SaveCurrentPageHtml(page, _path, pageName, pageIndexStr);

            bool moved = await MoveToNextPage(page);
            if (!moved)
            {
                Loggers.LogAs.Debug(_logger, "No more pages.");
                break;
            }

            pageIndex++;

            await RandomDelay(500, 1000);
        }

        Loggers.LogAs.Debug(_logger, "Done.");
    }
    #endregion

    private async Task GoToPageIndex(IPage page, string pageId)
    {
        if (page == null)
            return;

        await page.Locator("input[name=\"page\"]").ClickAsync();
        await page.Locator("input[name=\"page\"]").FillAsync(pageId);
        await page.GetByRole(AriaRole.Cell, new() { Name = $"{pageId} Go", Exact = true }).Locator("input[type=\"submit\"]").ClickAsync();

        Loggers.Services.Playwright.GoToPage(_logger, page.Url, pageId);
    }
    private async Task GoToPageUrl(IPage page, string url)
    {
        if (page == null)
            return;

        Loggers.Services.Playwright.GoToPage(_logger, url);

        await page.GotoAsync(url);
        await PageLoad(page);
    }
    private async Task PageLoad(IPage page)
    {
        if (page == null)
            return;

        Loggers.LogAs.Debug(_logger, "Waiting page to load ...");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    private async Task FillField(IPage page, string name, string value)
    {
        if (page == null)
            return;

        if (name != "Password")
            Loggers.Services.Playwright.FillField(_logger, name, value);
        else
            Loggers.Services.Playwright.FillField(_logger, name, Constants.Unsorted.PasswordMask);

        await page.GetByRole(AriaRole.Textbox, new() { Name = name }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = name }).FillAsync(value);
    }
    private async Task ClickButtonOnImage(IPage page, string name)
    {
        if (page == null)
            return;

        Loggers.Services.Playwright.ClickButton(_logger, name);
        name = name.ToLower();
        string? href = await page
        .Locator($"area[alt='{name}']")
        .GetAttributeAsync("href");

        await GoToPageUrl(page, new Uri(new Uri(page.Url), href!).ToString());
    }
    private async Task ClickButton(IPage page, string name)
    {
        if (page == null)
            return;

        Loggers.Services.Playwright.ClickButton(_logger, name);
        await page.GetByRole(AriaRole.Button, new() { Name = name }).ClickAsync();

        await PageLoad(page);
    }
    private async Task SaveCurrentPageHtml(IPage page, string path, string pageName, string pageIndex)
    {
        if (page == null)
            return;

        string html = await page.ContentAsync();
        string name = $"{pageName}_{pageIndex}";

        await _writter.SaveAsHtml(path, name, html);
    }
    private async Task<bool> MoveToNextPage(IPage page)
    {
        if (page == null)
            return false;

        ILocator? nextButton = page.Locator("a").Filter(new() { HasText = "Next" }).Last;

        if (await nextButton.CountAsync() == 0)
            return false;

        await nextButton.ClickAsync();
        return true;
    }
    public async Task RandomDelay(int minMs = 1200, int maxMs = 4500)
    {
        int delay = _random.Next(minMs, maxMs);
        Loggers.Services.Playwright.RandomDelay(_logger, delay);
        await Task.Delay(delay);
    }
}
