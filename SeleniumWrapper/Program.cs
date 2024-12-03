using System.IO.Compression;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NamedPipeWrapper;
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
                File.WriteAllText(Path.Combine(args[0] + "exit.txt"), "exit at " + DateTime.Now.ToString("HH:mm:ss"));
                _driver?.Close();
                _driver?.Quit();
            };
            AppDomain.CurrentDomain.UnhandledException += (_, _) =>
            {
                File.WriteAllText(Path.Combine(args[0] + "exit.txt"), "exit at " + DateTime.Now.ToString("HH:mm:ss"));
                _driver?.Close();
                _driver?.Quit();
            };

            var server = new NamedPipeServer<PipeMessage>("SeleniumWrapper");
            server.ClientConnected += OnClientConnected;
            server.ClientDisconnected += OnClientDisconnected;
            server.ClientMessage += OnClientMessage;
            server.Error += OnError;
            server.Start();

            void OnClientConnected(NamedPipeConnection<PipeMessage, PipeMessage> connection)
            {
                Console.WriteLine("Client connected");
            }

            void OnClientDisconnected(NamedPipeConnection<PipeMessage, PipeMessage> connection)
            {
                Console.WriteLine("Client disconnected");
            }
            
            void OnClientMessage(NamedPipeConnection<PipeMessage, PipeMessage> connection, PipeMessage message)
            {
                Console.WriteLine("Client message");
            }

            void OnError(Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }


            /* var namedPipe = new NamedPipeServerStream("SeleniumWrapper", PipeDirection.InOut, 1,
                PipeTransmissionMode.Message);
            await namedPipe.WaitForConnectionAsync();


            var pipeWriter = new StreamWriter(namedPipe, Encoding.UTF8)
            {
                AutoFlush = true
            };*/


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

            var devToolsSession = _driver.GetDevToolsSession();
            var fetch = devToolsSession
                .GetVersionSpecificDomains<OpenQA.Selenium.DevTools.V131.DevToolsSessionDomains>().Network;

            _ = fetch.Enable(new EnableCommandSettings());

            fetch.ResponseReceived += (_, e) =>
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

                            Console.WriteLine(authorName + ": " + message);
                            /*pipeWriter.WriteLine(authorName + ": " + message);

                            namedPipe.WaitForPipeDrain();*/

                            server.PushMessage(new PipeMessage(authorName.ToString(), message.ToString()));
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

            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    [Serializable]
    public record PipeMessage(string Author, string Message)
    {
        public string Author = Author;
        public string Message = Message;
    }
}