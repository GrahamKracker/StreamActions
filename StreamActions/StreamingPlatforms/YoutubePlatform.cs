using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using NamedPipeWrapper;
using UnityEngine;
using UnityEngine.U2D;

namespace StreamActions.StreamingPlatforms;


public class YoutubePlatform : StreamingPlatform
{
    private ModHelperInputField VideoIdInput { get; set; } = null!;
    private Process? _seleniumWrapper;
    
    public void KillSeleniumWrapper()
    {
        _seleniumWrapper?.Kill();
        Process.GetProcessesByName("SeleniumWrapper").ForEach(p => p.Close());
    }

    private static bool IsFileLocked(string filePath)
    {
        try
        {
            File.Create(filePath).Close();
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }

    private async Task SetupYoutube(string videoId)
    {
        KillSeleniumWrapper();
        await Task.Run(async () =>
        {
            while (IsFileLocked(Path.Combine(CacheFolder, "SeleniumWrapper.exe")))
            {
                await Task.Delay(25);
            }
        });

        try
        {
            var url = "https://www.youtube.com/live_chat?v=" + videoId;

            var seleniumWrapperFile = Path.Combine(CacheFolder, "SeleniumWrapper.exe");

            var wrapperResource = this.mod.MelonAssembly.Assembly.GetEmbeddedResource("SeleniumWrapper.exe");

            try
            {
                await using var fileStream = File.Create(seleniumWrapperFile);
                await wrapperResource.CopyToAsync(fileStream);
                fileStream.Close();
                wrapperResource.Close();
            }
            catch (IOException)
            {
                Main.Logger.Error("Failed to create SeleniumWrapper file.");
                throw;
            }

            _seleniumWrapper = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = seleniumWrapperFile,
                    Arguments = $"\"{CacheFolder}\" \"{url}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WorkingDirectory = CacheFolder,
                }
            };

            _seleniumWrapper.Start();

            Main.Logger.Msg("Started SeleniumWrapper");

            /*var namedPipeClient =
                new NamedPipeClientStream(".", "SeleniumWrapper", PipeDirection.InOut);
            await namedPipeClient.ConnectAsync();
            namedPipeClient.ReadMode = PipeTransmissionMode.Message;

            var pipeReader = new StreamReader(namedPipeClient, Encoding.UTF8, default, 1024, true);

            Main.Logger.Msg("Connected to SeleniumWrapper");

            _ = Task.Factory.StartNew(() =>
            {
                while (namedPipeClient.IsConnected)
                {
                    var message = pipeReader.ReadLine();
                    if (message != null)
                    {
                        var split = message.Split(":", 2);

                        OnMessageReceived(split[0], split[1]);
                    }
                }

                namedPipeClient.Dispose();
                pipeReader.Dispose();

                Main.Logger.Msg("Disconnected from YouTube.");
            }, TaskCreationOptions.LongRunning);*/

            /*var namedPipeClient =
                new NamedPipeClientStream(".", "SeleniumWrapper", PipeDirection.InOut);
            await namedPipeClient.ConnectAsync();
            namedPipeClient.ReadMode = PipeTransmissionMode.Message;

            var pipeReader = new StreamReader(namedPipeClient, Encoding.UTF8, default, 1024, true);*/

            var client = new NamedPipeClient<PipeMessage>("SeleniumWrapper");
            client.ServerMessage += OnServerMessage;
            client.Error += OnError;
            client.Start();

            void OnServerMessage(NamedPipeConnection<PipeMessage, PipeMessage> connection, PipeMessage message)
            {
                OnMessageReceived(message.Author, message.Message);
            }

            void OnError(Exception exception)
            {
                Main.Logger.Error(exception);
            }

            Main.Logger.Msg("Connected to SeleniumWrapper");

            /*_ = Task.Factory.StartNew(() =>
            {
                while (namedPipeClient.IsConnected)
                {
                    var message = pipeReader.ReadLine();
                    if (message != null)
                    {
                        var split = message.Split(":", 2);

                        OnMessageReceived(split[0], split[1]);
                    }
                }

                namedPipeClient.Dispose();
                pipeReader.Dispose();

                Main.Logger.Msg("Disconnected from YouTube.");
            }, TaskCreationOptions.LongRunning);*/

            PopupScreen.instance.SafelyQueue(p => { p.ShowOkPopup("Connected to YouTube video"); });

            if (Settings.SaveToken && VideoIdInput?.InputField?.text != null)
            {
                SaveToCache();
            }
        }
        catch (Exception e)
        {
            Main.Logger.Error(e);
        }
    }

    public override async void ConnectToPlatform()
    {
        try
        {
            Main.SendAnalytics(Main.AnalyticsAction.ManualYoutube);
            await SetupYoutube(VideoIdInput.InputField.text);
        }
        catch (Exception e)
        {
            Main.Logger.Error(e);
        }
    }

    private void OnMessageReceived(string author, string message)
    {
        if (ChannelsAnswered.Add(author))
            ChatMessageReceived(message);
    }

    /// <inheritdoc />
    protected override string[] CacheData => [VideoIdInput.InputField.text];

    /// <inheritdoc />
    protected override bool LoadFromCacheData(string[] lines)
    {
        SetupYoutube(lines[0]).Wait();
        Main.SendAnalytics(Main.AnalyticsAction.AutoYoutube);
        return true;
    }

    /// <inheritdoc />
    public override void CreateConnectPanel(ModHelperPanel rightMenu)
    {
        const float height = 200;
        const float labelWidth = 300;

        var videoPanel = rightMenu.AddPanel(new Info("IdPanel")
        {
            Width = rightMenu.initialInfo.Width - 50 * 2,
            Height = 200,
        }, null, RectTransform.Axis.Horizontal, 100);
        videoPanel.AddText(new Info("ChannelLabel", labelWidth, height), "Video Id: ", 65).Text.enableAutoSizing =
            true;
        VideoIdInput = videoPanel.AddInputField(new Info("TokenInput")
            {
                FlexWidth = 1,
                Height = height,
            }, "",
            VanillaSprites.BlueInsertPanelRound, null, 65);

        rightMenu.AddButton(new Info("ConnectButton", 600, 250), VanillaSprites.GreenBtnLong,
            new System.Action(ConnectToPlatform)).AddText(new Info("Text", InfoPreset.FillParent), "Connect", 69);
    }

    ~YoutubePlatform() => KillSeleniumWrapper();

    [Serializable]
    public record PipeMessage(string Author, string Message)
    {
        public string Author = Author;
        public string Message = Message;
    }
}