using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using H.Formatters;
using H.Pipes;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V135.Network;
using OpenQA.Selenium.Edge;

namespace SeleniumWrapper;

class Program
{
    private static EdgeDriver _driver = null!;

    static async Task Main(string[] args)
    {
        try
        {
            AppDomain.CurrentDomain.ProcessExit += (_, _) => { _driver?.Quit(); };
            AppDomain.CurrentDomain.UnhandledException += (_, _) =>
            {
                File.WriteAllText(Path.Combine(args[0] + "error.txt"), "exit at " + DateTime.Now.ToString("HH:mm:ss"));
                _driver?.Quit();
            };

            var server = new PipeServer<PipeMessage>("SeleniumWrapper", formatter: new NewtonsoftJsonFormatter());

            server.ClientConnected += (o, args) =>
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

            if (!Directory.Exists(cacheFolder))
                Directory.CreateDirectory(cacheFolder);

            DriverHandler.DownloadLatest(cacheFolder);

            var options = new EdgeOptions();

            if (args.Length < 3 || args[2] == "false")
            {
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--headless");
            }

            _driver = new EdgeDriver(Path.Combine(cacheFolder, "edgedriver_win64", "msedgedriver.exe"), options);
            DevToolsSession devToolsSession;
            try
            {
                devToolsSession = _driver.GetDevToolsSession();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }

            var fetch = devToolsSession
                .GetVersionSpecificDomains<OpenQA.Selenium.DevTools.V135.DevToolsSessionDomains>().Network;

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

                            await server.WriteAsync(new PipeMessage(MessageType.ChatMessage, new Dictionary<string, string>(){
                                ["Author"] = authorName.ToString(),
                                ["Message"] = message.ToString(),
                            }));
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

            if (_driver.FindElements(By.XPath("/html/body/yt-live-chat-app/div/yt-live-chat-message-renderer/yt-formatted-string")).FirstOrDefault() != null
                || _driver.FindElements(By.XPath("/html/body/div/h1")).FirstOrDefault() != null)
            {
                Console.WriteLine("Error: No chat available for livestream, or livestream has ended.");
                await server.WriteAsync(new PipeMessage(MessageType.Error, new Dictionary<string, string>(){
                    ["Error"] = "No chat available for livestream, or livestream has ended."
                }));
                await server.StopAsync();
                _driver.Quit();
                return;
            }

            try
            {
                await Task.Delay(4000);
                _driver.FindElements(By.XPath("//*[@id=\"label-text\"]")).FirstOrDefault()?.Click();
                await Task.Delay(500);
                _driver.FindElements(By.XPath("/html/body/yt-live-chat-app/div/yt-live-chat-renderer/iron-pages/div/yt-live-chat-header-renderer/div[1]/span[2]/yt-sort-filter-sub-menu-renderer/yt-dropdown-menu/tp-yt-paper-menu-button/tp-yt-iron-dropdown/div/div/tp-yt-paper-listbox/a[2]/tp-yt-paper-item")).FirstOrDefault()?.Click();
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

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private record PipeMessage(MessageType Type, Dictionary<string, string> Payload)
    {
        public readonly MessageType Type = Type;
        public readonly Dictionary<string, string> Payload = Payload;
    }

    private enum MessageType : byte
    {
        ChatMessage = 0,
        Error = 1,
        SuccessfulConnection = 2,
    }

    ~Program() => _driver?.Quit();
}