using System.IO.Compression;
using System.IO.Pipes;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V131.Network;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.Events;
using RequestPausedEventArgs = OpenQA.Selenium.DevTools.V131.Fetch.RequestPausedEventArgs;

namespace SeleniumWrapper;

class Program
{
    private static EdgeDriver _driver = null!;



    static async Task Main(string[] args)
    {
        try
        {
            var namedPipe = new NamedPipeServerStream("SeleniumWrapper", PipeDirection.Out, 1);
            //await namedPipe.WaitForConnectionAsync().ConfigureAwait(false);

            var CacheFolder = args[0];
            var url = args[1]; // "https://www.youtube.com/live_chat?v=FNGAvwzgKf4";

            using HttpClient client = new();

            if (!File.Exists(Path.Combine(CacheFolder, "edgedriver_win64", "msedgedriver.exe")))
            {
                var response =
                    await client.GetAsync("https://msedgedriver.azureedge.net/131.0.2903.70/edgedriver_win64.zip");
                await using var stream = await response.Content.ReadAsStreamAsync();

                await using var fileStream = File.Create(Path.Combine(CacheFolder, "edgedriver_win64.zip"));

                await stream.CopyToAsync(fileStream);

                fileStream.Close();

                //extract the zip

                ZipFile.ExtractToDirectory(Path.Combine(CacheFolder, "edgedriver_win64.zip"),
                    Path.Combine(CacheFolder, "edgedriver_win64"));

                File.Delete(Path.Combine(CacheFolder, "edgedriver_win64.zip"));

                Directory.Delete(Path.Combine(CacheFolder, "edgedriver_win64", "Driver_Notes"), true);
            }

            var options = new EdgeOptions
            {
                UseWebSocketUrl = true,
            };

            /*options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--headless");*/

            _driver = new EdgeDriver(Path.Combine(CacheFolder, "edgedriver_win64", "msedgedriver.exe"), options);

            //await DomChanges();
            /*NetworkManager manager = new NetworkManager(_driver);
            await manager.StartMonitoring();

            DevToolsSession session = _driver.GetDevToolsSession();
            session.Domains.Network.ResponsePaused += (_, eventArgs) =>
            {
                Console.WriteLine("response: " + eventArgs.ResponseData.Url);
                return session.Domains.Network.ContinueResponseWithoutModification(eventArgs.ResponseData);
            };
            await session.Domains.Network.EnableNetwork();*/



            //https://duckduckgo.com/?t=ffab&q=WARN+DevToolsSession%3A+CDP+VNT+%5E%5E+Unhandled+error+occured+in+event+handler+of+%27Fetch.requestPaused%27+method.+OpenQA.Selenium.DevTools.CommandResponseException%3A+Fetch.continueResponse%3A+Invalid+InterceptionId.+++++++++++at+OpenQA.Selenium.DevTools.DevToolsSession.SendCommand(String+commandName%2C+String+sessionId%2C+JsonNode+commandParameters%2C+CancellationToken+cancellationToken%2C+Nullable%601+millisecondsTimeout%2C+Boolean+throwExceptionIfResponseNotReceived)+&ia=web

            var webDriver = _driver;

            Request();
            //Response();


            await _driver.Navigate().GoToUrlAsync(url);

            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void Request()
    {
        DevToolsSession devToolsSession = _driver.GetDevToolsSession();

        var fetch = devToolsSession.GetVersionSpecificDomains<OpenQA.Selenium.DevTools.V131.DevToolsSessionDomains>()
            .Fetch;
        var enableCommandSettings = new OpenQA.Selenium.DevTools.V131.Fetch.EnableCommandSettings();
        OpenQA.Selenium.DevTools.V131.Fetch.RequestPattern requestPattern =
            new OpenQA.Selenium.DevTools.V131.Fetch.RequestPattern
            {
                RequestStage = OpenQA.Selenium.DevTools.V131.Fetch.RequestStage.Request,
            };

        enableCommandSettings.Patterns = [requestPattern];
        _ = fetch.Enable(enableCommandSettings);

        void RequestIntercepted(object? sender, RequestPausedEventArgs e)
        {
            Console.WriteLine(e.Request.Url);
                Console.WriteLine(e.Request.Method);
                fetch.ContinueRequest(new OpenQA.Selenium.DevTools.V131.Fetch.ContinueRequestCommandSettings()
                { RequestId = e.RequestId });
        }

        fetch.RequestPaused += RequestIntercepted;
    }

    private static void Response()
    {
        DevToolsSession devToolsSession = _driver.GetDevToolsSession();

        var fetch = devToolsSession.GetVersionSpecificDomains<OpenQA.Selenium.DevTools.V131.DevToolsSessionDomains>().Network;
        var enableCommandSettings = new OpenQA.Selenium.DevTools.V131.Network.SetRequestInterceptionCommandSettings();

        fetch = [requestPattern];
        _ = fetch.Enable(enableCommandSettings);

        void ResponseIntercepted(object? sender, ResponseReceivedEventArgs e)
        {
            Console.WriteLine(e.Response.Url);
        }

        fetch.ResponseReceived += ResponseIntercepted;
    }

    private static async Task DomChanges()
    {
        JavaScriptEngine monitor = new JavaScriptEngine(_driver);
        monitor.DomMutated += (_, e) =>
        {
            try
            {
                if (e.AttributeData?.Element == null)
                    return;
                var element = e.AttributeData.Element;

                if (element.TagName != "yt-live-chat-text-message-renderer")
                    return;

                var author = element.FindElements(By.CssSelector("#author-name"));
                var text = element.FindElements(By.CssSelector("#message"));

                if (author == null || author.Count == 0)
                {
                    return;
                }

                if (text == null || text.Count == 0)
                {
                    return;
                }

                Console.WriteLine(author[0].Text + ": " + text[0].Text);

                //namedPipe.Write(Encoding.UTF8.GetBytes(e.AttributeData.Element.Text));
            }
            catch (StaleElementReferenceException)
            {
                // ignored
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                throw;
            }

        };
        await monitor.EnableDomMutationMonitoring();
        await monitor.StartEventMonitoring();
    }

    private void GetMessages()
    {
        var messages = _driver.FindElements(By.CssSelector("yt-live-chat-text-message-renderer"));
        foreach (var message in messages)
        {
            var author = message.FindElement(By.CssSelector("#author-name")).Text;
            var text = message.FindElement(By.CssSelector("#message")).Text;
        }
    }
}