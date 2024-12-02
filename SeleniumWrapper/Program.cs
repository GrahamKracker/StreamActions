using System.IO.Compression;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
                File.WriteAllText(Path.Combine(args[0] + "exit.txt"), "exit at " + DateTime.Now.ToString("HH:mm:ss"));
                _driver?.Quit();
            };
            AppDomain.CurrentDomain.UnhandledException += (_, _) =>
            {
                File.WriteAllText(Path.Combine(args[0] + "exit.txt"), "exit at " + DateTime.Now.ToString("HH:mm:ss"));
                _driver?.Quit();
            };

            var namedPipe = new NamedPipeServerStream("SeleniumWrapper", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            await namedPipe.WaitForConnectionAsync().ConfigureAwait(false);

            var writer = new StreamWriter(namedPipe, Encoding.UTF8, bufferSize: 2048, leaveOpen: true);

            var CacheFolder = args[0];
            var url = args[1];

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

            var options = new EdgeOptions();

            /*options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--headless");*/

            _driver = new EdgeDriver(Path.Combine(CacheFolder, "edgedriver_win64", "msedgedriver.exe"), options);

            DevToolsSession devToolsSession = _driver.GetDevToolsSession();

            var fetch = devToolsSession.GetVersionSpecificDomains<OpenQA.Selenium.DevTools.V131.DevToolsSessionDomains>().Network;

            _ = fetch.Enable(new EnableCommandSettings());

            fetch.ResponseReceived += (_, e) =>
            {
                try
                {
                    if(!e.Response.Url.Contains("live_chat/get_live_chat"))
                        return;

                    fetch.GetResponseBody(new GetResponseBodyCommandSettings()
                    {
                        RequestId = e.RequestId
                    }, default, null, false).ContinueWith(async task =>
                    {
                        var body = task.Result.Body;
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

                                var authorName = action["addChatItemAction"]?["item"]?["liveChatTextMessageRenderer"]?["authorName"]?["simpleText"];
                                var message = action["addChatItemAction"]?["item"]?["liveChatTextMessageRenderer"]?["message"]?["runs"]?[0]?["text"];
                                if (authorName == null || message == null) continue;

                                Console.WriteLine(authorName + ": " + message);
                                await writer.WriteLineAsync(authorName + ": " + message);
                                await writer.FlushAsync();
                            }
                        }
                        catch (JsonException)
                        {
                            // ignored
                        }
                    });
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            };

            await _driver.Navigate().GoToUrlAsync(url);

            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}