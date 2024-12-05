using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using H.Formatters;
using H.Pipes;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using JetBrains.Annotations;
using UnityEngine;

namespace StreamActions.StreamingPlatforms;

public class  YoutubePlatform : StreamingPlatform
{
    private ModHelperInputField VideoIdInput { get; set; } = null!;
    private Process? _seleniumWrapper;
    
    public void KillSeleniumWrapper()
    {
        _seleniumWrapper?.Kill();
        Process.GetProcessesByName("SeleniumWrapper").ForEach(p =>
        {
            ModLogger.Msg("Killing SeleniumWrapper process");
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

            var wrapperResource = mod.MelonAssembly.Assembly.GetEmbeddedResource("SeleniumWrapper.exe");

            try
            {
                await using var fileStream = File.Create(seleniumWrapperFile);
                await wrapperResource.CopyToAsync(fileStream);
                fileStream.Close();
                wrapperResource.Close();
            }
            catch (IOException)
            {
                ModLogger.Error("Failed to create SeleniumWrapper file.");
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

            ModLogger.Msg("Starting SeleniumWrapper exe");

            var client = new PipeClient<PipeMessage>("SeleniumWrapper", formatter:new NewtonsoftJsonFormatter());
            client.MessageReceived += (o, args) =>
            {
                if (args.Message is { Type: MessageType.ChatMessage })
                    OnMessageReceived(args.Message.Payload["Author"], args.Message.Payload["Message"]);
                if (args.Message is { Type: MessageType.Error })
                {
                    ModLogger.Error("Error from SeleniumWrapper: " + args.Message.Payload["Error"]);
                    PopupScreen.instance.SafelyQueue(p => { p.ShowOkPopup(
                        $"Error from YouTube video connection: {args.Message.Payload["Error"]}"); });
                    KillSeleniumWrapper();
                }
            };
            client.Disconnected += (o, args) =>
            {
                ModLogger.Warning("Disconnected from SeleniumWrapper named pipe");
                KillSeleniumWrapper();
            };
            client.Connected += (o, args) => ModLogger.Msg("Connected to SeleniumWrapper named pipe");
            client.ExceptionOccurred += (o, args) =>
            {
                ModLogger.Error("Exception occurred in SeleniumWrapper while trying to send data through the named pipe");
                ModLogger.Error(args.Exception.ToString());
                PopupScreen.instance.SafelyQueue(p => { p.ShowOkPopup("Failure in the connection to YouTube video"); });
                KillSeleniumWrapper();
            };

            await client.ConnectAsync();

            PopupScreen.instance.SafelyQueue(p => { p.ShowOkPopup("Successfully connected to video browser client"); });

            if (Settings.SaveToCache && VideoIdInput?.InputField?.text != null)
            {
                SaveToCache();
            }
        }
        catch (Exception e)
        {
            ModLogger.Error(e);
        }
    }

    public override async void ConnectToPlatform()
    {
        try
        {
            SendAnalytics(AnalyticsAction.ManualYoutube);
            await SetupYoutube(VideoIdInput.InputField.text);
        }
        catch (Exception e)
        {
            ModLogger.Error(e);
        }
    }

    /// <inheritdoc />
    protected override string[] CacheData => [VideoIdInput.InputField.text];

    /// <inheritdoc />
    protected override bool LoadFromCacheData(string[] lines)
    {
        SetupYoutube(lines[0]).Wait();
        SendAnalytics(AnalyticsAction.AutoYoutube);
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
    }

    /// <inheritdoc />
    public override void Disconnect()
    {
        KillSeleniumWrapper();
    }

    ~YoutubePlatform() => KillSeleniumWrapper();

    // ReSharper disable once ClassNeverInstantiated.Local
    private record PipeMessage(MessageType Type, Dictionary<string, string> Payload)
    {
        public readonly MessageType Type = Type;
        public readonly Dictionary<string, string> Payload = Payload;
    }

    private enum MessageType : byte
    {
        ChatMessage = 0,
        Error = 1,
    }
}