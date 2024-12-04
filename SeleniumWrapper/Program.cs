using System.IO.Compression;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using H.Formatters;
using H.Pipes;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V131.Network;
using OpenQA.Selenium.Edge;

namespace SeleniumWrapper;

class Program
{
    private static EdgeDriver _driver = null!;

    static async Task Main(string[] args)
    {
        try
        {
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                _driver?.Quit();
            };
            AppDomain.CurrentDomain.UnhandledException += (_, _) =>
            {
                File.WriteAllText(Path.Combine(args[0] + "error.txt"), "exit at " + DateTime.Now.ToString("HH:mm:ss"));
                _driver?.Quit();
            };

            var server = new PipeServer<PipeMessage>("SeleniumWrapper", formatter: new NewtonsoftJsonFormatter());

            server.ClientConnected +=  (o, args) =>
            {
                Console.WriteLine($"Client {args.Connection.PipeName} is now connected!");
            };
            server.ClientDisconnected += (o, args) =>
            {
                Console.WriteLine($"Client {args.Connection.PipeName} disconnected");
            };
            server.MessageReceived += (sender, args) =>
            {
                Console.WriteLine($"Client {args.Connection.PipeName} says: {args.Message}");
            };
            server.ExceptionOccurred += (o, args) => Console.WriteLine(args.Exception);

            await server.StartAsync();

            var cacheFolder = args[0];
            var url = args[1];

            using HttpClient client = new();

            if (!File.Exists(Path.Combine(cacheFolder, "edgedriver_win64", "msedgedriver.exe")))
            {
                var response =
                    await client.GetAsync("https://msedgedriver.azureedge.net/131.0.2903.70/edgedriver_win64.zip");
                await using var stream = await response.Content.ReadAsStreamAsync();

                await using var fileStream = File.Create(Path.Combine(cacheFolder, "edgedriver_win64.zip"));

                await stream.CopyToAsync(fileStream);

                fileStream.Close();

                //extract the zip

                ZipFile.ExtractToDirectory(Path.Combine(cacheFolder, "edgedriver_win64.zip"),
                    Path.Combine(cacheFolder, "edgedriver_win64"));

                File.Delete(Path.Combine(cacheFolder, "edgedriver_win64.zip"));

                Directory.Delete(Path.Combine(cacheFolder, "edgedriver_win64", "Driver_Notes"), true);
            }

            var options = new EdgeOptions();

            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--headless");


            _driver = new EdgeDriver(Path.Combine(cacheFolder, "edgedriver_win64", "msedgedriver.exe"), options);

            var devToolsSession = _driver.GetDevToolsSession();
            var fetch = devToolsSession
                .GetVersionSpecificDomains<OpenQA.Selenium.DevTools.V131.DevToolsSessionDomains>().Network;

            _ = fetch.Enable(new EnableCommandSettings());

            fetch.ResponseReceived += async (_, e) =>
            {
                try
                {
                    if (!e.Response.Url.Contains("live_chat/get_live_chat"))
                        return;

                    string? body;

                    try
                    {
                        var getResponseBodyTask = fetch.GetResponseBody(new GetResponseBodyCommandSettings()
                        {
                            RequestId = e.RequestId
                        }, CancellationToken.None, null, false);
                        body = getResponseBodyTask.GetAwaiter().GetResult().Body;
                    }
                    catch (CommandResponseException)
                    {
                        return;
                    }

                    //try parse to JObject
                    try
                    {
                        var json = JsonNode.Parse(body)?.AsObject();
                        var actions = json?["continuationContents"]?["liveChatContinuation"]?["actions"]?.AsArray();
                        if (actions == null)
                            return;
                        foreach (var action in actions)
                        {
                            if (action == null)
                                continue;

                            var authorName =
                                action["addChatItemAction"]?["item"]?["liveChatTextMessageRenderer"]?["authorName"]?[
                                    "simpleText"];
                            var message =
                                action["addChatItemAction"]?["item"]?["liveChatTextMessageRenderer"]?["message"]?[
                                    "runs"]?[0]?["text"];
                            if (authorName == null || message == null) continue;

                            await server.WriteAsync(new PipeMessage(authorName.ToString(), message.ToString()));
                        }
                    }
                    catch (JsonException)
                    {
                        // ignored
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            };

            await _driver.Navigate().GoToUrlAsync(url);

            //todo: detect if livestream is actually running and if not, cancel and return, telling the mod that it doesnt exist too

            try
            {
                await Task.Delay(4000);
                _driver.FindElement(By.XPath("//*[@id=\"label-text\"]")).Click();
                await Task.Delay(500);
                _driver.FindElement(By.XPath("/html/body/yt-live-chat-app/div/yt-live-chat-renderer/iron-pages/div/yt-live-chat-header-renderer/div[1]/span[2]/yt-sort-filter-sub-menu-renderer/yt-dropdown-menu/tp-yt-paper-menu-button/tp-yt-iron-dropdown/div/div/tp-yt-paper-listbox/a[2]/tp-yt-paper-item")).Click();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private record PipeMessage(string Author, string Message)
    {
        // ReSharper disable once UnusedMember.Local
        public string Author = Author;
        // ReSharper disable once UnusedMember.Local
        public string Message = Message;
    }

    ~Program() => _driver?.Quit();
}