// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Library.Tests.given;

/// <summary>
/// Drives a frontend through nothing but the <c>data-testid</c>s both of them agree on. There is deliberately no
/// branching on which frontend is being driven — the base address is the only thing that differs, and everything
/// below waits for an outcome rather than assuming one, which is what lets a server-rendered form post and a
/// client-side fetch be expressed the same way.
/// </summary>
public sealed partial class FrontendDriver : IAsyncDisposable
{
    static readonly TimeSpan _patience = TimeSpan.FromSeconds(30);

    // Long enough for either frontend to have rendered a rejection, short enough that asking "and none appeared?"
    // does not cost half a minute every time the answer is no.
    static readonly TimeSpan _glance = TimeSpan.FromSeconds(5);

    readonly IBrowserContext _context;
    readonly IPage _page;

    FrontendDriver(IBrowserContext context, IPage page)
    {
        _context = context;
        _page = page;
    }

    /// <summary>
    /// Opens a frontend and lands on the authors page — the first navigation item, so the shell is rendered and the
    /// <c>frontend-kind</c> badge can be read before anything else happens.
    /// </summary>
    /// <param name="browser">The <see cref="IBrowser"/> to open a context in.</param>
    /// <param name="baseAddress">The address the frontend is served from.</param>
    /// <returns>The <see cref="FrontendDriver"/> on the authors page.</returns>
    public static async Task<FrontendDriver> Open(IBrowser browser, Uri baseAddress)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = baseAddress.ToString(),
            IgnoreHTTPSErrors = true
        });

        var driver = new FrontendDriver(context, await context.NewPageAsync());
        await driver.GoTo(PagePaths.Authors);

        return driver;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync() => await _context.DisposeAsync();

    /// <summary>
    /// Navigates to a page and waits for the shell to be rendered. The single-page frontend has to boot its
    /// JavaScript before the badge exists, so waiting for the badge is what "the page is ready" means for both.
    /// </summary>
    /// <param name="path">The path to navigate to.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task GoTo(string path)
    {
        await _page.GotoAsync(path, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await _page.GetByTestId(TestIds.FrontendKind).WaitForAsync(Waiting());
    }

    /// <summary>
    /// Reads the badge naming the frontend being driven, so a failing spec says which one broke.
    /// </summary>
    /// <returns>The frontend's name.</returns>
    public async Task<string> FrontendKind() =>
        (await _page.GetByTestId(TestIds.FrontendKind).InnerTextAsync()).Trim();

    /// <summary>
    /// Types a value into a field.
    /// </summary>
    /// <param name="testId">The <c>data-testid</c> of the field.</param>
    /// <param name="value">The value to type.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Fill(string testId, string value) => _page.GetByTestId(testId).FillAsync(value);

    /// <summary>
    /// Picks an option by its value. Every picker is populated from the API with the integer id or the ISBN as the
    /// option value, in both frontends.
    /// </summary>
    /// <param name="testId">The <c>data-testid</c> of the picker.</param>
    /// <param name="value">The option value to pick.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Pick(string testId, string value)
    {
        var picker = _page.GetByTestId(testId);

        // The options arrive from the API, so the one being picked may not be there the instant the page renders.
        await picker.Locator($"option[value='{value}']").WaitForAsync(Waiting());
        await picker.SelectOptionAsync(new SelectOptionValue { Value = value });
    }

    /// <summary>
    /// Presses a button.
    /// </summary>
    /// <param name="testId">The <c>data-testid</c> of the button.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Press(string testId) => _page.GetByTestId(testId).ClickAsync();

    /// <summary>
    /// Reads the rejection shown on the page, waiting for it to appear.
    /// </summary>
    /// <returns>The message, or an empty string when no rejection was shown.</returns>
    public async Task<string> Rejection()
    {
        var error = _page.GetByTestId(TestIds.Error).First;

        return await Appears(error)
            ? (await error.InnerTextAsync()).Trim()
            : string.Empty;
    }

    /// <summary>
    /// Determines whether the page settled without rejecting what was just asked of it.
    /// </summary>
    /// <returns>True when no rejection was shown.</returns>
    public async Task<bool> Accepted() => !await Appears(_page.GetByTestId(TestIds.Error).First, _glance);

    /// <summary>
    /// Determines whether a table holds a row containing the given text, waiting for it to turn up.
    /// </summary>
    /// <param name="table">The <c>data-testid</c> of the table.</param>
    /// <param name="row">The <c>data-testid</c> of its rows.</param>
    /// <param name="text">The text the row is recognized by.</param>
    /// <returns>True when the row is there, false when it never turned up.</returns>
    public Task<bool> HasRow(string table, string row, string text) => Appears(Row(table, row, text));

    /// <summary>
    /// Reads the key of the row containing the given text. Every row carries its integer key as <c>data-id</c>,
    /// which is what the pickers on the other pages are populated with.
    /// </summary>
    /// <param name="table">The <c>data-testid</c> of the table.</param>
    /// <param name="row">The <c>data-testid</c> of its rows.</param>
    /// <param name="text">The text the row is recognized by.</param>
    /// <returns>The row's key.</returns>
    /// <exception cref="RowNotFound">Thrown when no such row turns up, or it carries no key.</exception>
    public async Task<string> KeyOf(string table, string row, string text)
    {
        var located = Row(table, row, text);

        if (!await Appears(located))
        {
            throw new RowNotFound(table, text);
        }

        return await located.GetAttributeAsync("data-id") ?? throw new RowNotFound(table, text);
    }

    /// <summary>
    /// Presses a button inside the row containing the given text.
    /// </summary>
    /// <param name="table">The <c>data-testid</c> of the table.</param>
    /// <param name="row">The <c>data-testid</c> of its rows.</param>
    /// <param name="text">The text the row is recognized by.</param>
    /// <param name="button">The <c>data-testid</c> of the button in the row.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task PressInRow(string table, string row, string text, string button) =>
        Row(table, row, text).GetByTestId(button).ClickAsync();

    /// <summary>
    /// Determines whether the row containing the given text offers a button, waiting for the page to settle first.
    /// Used to tell an open loan from a returned one without knowing how either renders.
    /// </summary>
    /// <param name="table">The <c>data-testid</c> of the table.</param>
    /// <param name="row">The <c>data-testid</c> of its rows.</param>
    /// <param name="text">The text the row is recognized by.</param>
    /// <param name="button">The <c>data-testid</c> of the button in the row.</param>
    /// <returns>True when the button is offered.</returns>
    public async Task<bool> RowOffers(string table, string row, string text, string button)
    {
        var located = Row(table, row, text);

        return await Appears(located) && await located.GetByTestId(button).CountAsync() > 0;
    }

    static LocatorWaitForOptions Waiting() => Waiting(_patience);

    static LocatorWaitForOptions Waiting(TimeSpan patience) => new() { Timeout = (float)patience.TotalMilliseconds };

    static async Task<bool> Appears(ILocator locator) => await Appears(locator, _patience);

    static async Task<bool> Appears(ILocator locator, TimeSpan patience)
    {
        try
        {
            await locator.WaitForAsync(Waiting(patience));
            return true;
        }
        catch (PlaywrightException error) when (error.Message.Contains("Timeout", StringComparison.Ordinal))
        {
            // Whether it turned up is the question being asked, so not turning up is an answer rather than a fault.
            // Anything Playwright raises for another reason is a real problem and keeps travelling.
            return false;
        }
    }

    ILocator Row(string table, string row, string text) =>
        _page.GetByTestId(table).GetByTestId(row).Filter(new LocatorFilterOptions { HasText = text }).First;
}
