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
using H.Formatters;
using H.Pipes;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using UnityEngine;

namespace StreamActions.StreamingPlatforms;


public class YoutubePlatform : StreamingPlatform
{
    private ModHelperInputField VideoIdInput { get; set; } = null!;
    private Process? _seleniumWrapper;
    
    public void KillSeleniumWrapper()
    {
        _seleniumWrapper?.Kill();
        Process.GetProcessesByName("SeleniumWrapper").ForEach(p =>
        {
            Main.Logger.Msg("Killing SeleniumWrapper process");
            p.Close();
        });
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
        await Task.Run(async () =>
        {
            int attempts = 0;
            while (IsFileLocked(Path.Combine(CacheFolder, "SeleniumWrapper.exe")))
            {
                await Task.Delay(25);
                if(attempts++ % 3 == 0)
                    KillSeleniumWrapper();
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
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = CacheFolder,
                }
            };

            _seleniumWrapper.Start();

            Main.Logger.Msg("Starting SeleniumWrapper exe");

            var client = new PipeClient<PipeMessage>("SeleniumWrapper", formatter:new NewtonsoftJsonFormatter());
            client.MessageReceived += (o, args) =>
            {
                if (args.Message != null)
                    OnMessageReceived(args.Message.Author, args.Message.Message);
            };
            client.Disconnected += (o, args) =>
            {
                Main.Logger.Warning("Disconnected from SeleniumWrapper named pipe");
                PopupScreen.instance.SafelyQueue(p => { p.ShowOkPopup("Disconnected from YouTube video"); });
                KillSeleniumWrapper();
            };
            client.Connected += (o, args) => Main.Logger.Msg("Connected to SeleniumWrapper named pipe");
            client.ExceptionOccurred += (o, args) =>
            {
                Main.Logger.Error("Exception occurred in SeleniumWrapper while trying to send data through the named pipe");
                Main.Logger.Error(args.Exception.ToString());
                PopupScreen.instance.SafelyQueue(p => { p.ShowOkPopup("Failure in the connection to YouTube video"); });
                KillSeleniumWrapper();
            };

            await client.ConnectAsync();

            PopupScreen.instance.SafelyQueue(p => { p.ShowOkPopup("Connected to YouTube video"); });

            if (Settings.SaveToCache && VideoIdInput?.InputField?.text != null)
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

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed record PipeMessage(string Author, string Message)
    {
        public readonly string Author = Author;
        public readonly string Message = Message;
    }
}