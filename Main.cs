using System;
using System.IO;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.UI_New.Main;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppTMPro;
using MelonLoader.Utils;
using StreamActions;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(StreamActions.Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace StreamActions;

[HarmonyPatch]
public class Main : BloonsTD6Mod
{
    internal static MelonLogger.Instance Logger;

    public static string ModFolder = Path.Combine(MelonEnvironment.ModsDirectory, "StreamActions");
    public static string CacheFolder = Path.Combine(ModFolder, "Cache");

    public static TwitchClient? client;

    private static string TwitchUserName = "grahamkracker1";

    public override void OnInitialize()
    {
        Logger = LoggerInstance;
    }

    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Start))]
    [HarmonyPostfix]
    private static void MainMenu_Start(MainMenu __instance)
    {
        MainMenuButton.Create(__instance);
    }

    /// <inheritdoc />
    public override void OnTitleScreen()
    {
        if (Settings.SaveTokenToFile)
        {
            ConnectToTwitch(FromCache());
        }
    }

    private static ConnectionCredentials FromCache() => new(TwitchUserName, File.ReadAllLines(Path.Combine(CacheFolder, "auth.txt"))[0]);

    public void ConnectToTwitch(ConnectionCredentials credentials)
    {
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };

        client = new TwitchClient(new WebSocketClient(clientOptions));
        client.Initialize(credentials, TwitchUserName);

        client.OnMessageReceived += Client_OnMessageReceived;

        client.Connect();
        MelonLogger.Msg("connected");
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        MelonLogger.Msg(e.ChatMessage.Message);
    }

    public void CreateSignInPopup()
    {
        PopupScreen.instance.SafelyQueue(p =>
        {
            ModHelperInputField channelInput = null!;
            ModHelperInputField tokenInput = null!;

            p.ShowPopup(PopupScreen.Placement.menuCenter,
                "Sign in to Twitch","Go to https://twitchapps.com/tmi/ to get your token:",
                new Action(() =>
                {
                    ConnectToTwitch(new ConnectionCredentials(TwitchUserName, tokenInput.InputField.text));
                }), "Confirm", null, "Cancel", Popup.TransitionAnim.Scale);

            TaskScheduler.ScheduleTask(() =>
            {
                var panel = p.GetFirstActivePopup().bodyObj.AddModHelperPanel(new Info("StreamActionPanel", InfoPreset.Flex), VanillaSprites.BlueInsertPanelRound, RectTransform.Axis.Vertical, 0);

                var channelPanel = panel.AddPanel(new Info("ChannelPanel",
                    1700, 150F * 1.5f, new Vector2(.5f, 0.1f)), null, RectTransform.Axis.Horizontal, 100);
                channelPanel.AddText(new Info("ChannelLabel", 200, 150F * 1.5f), "Channel: ", 65);
                channelInput = channelPanel.AddInputField(new Info("TokenInput", 1700, 150F * 1.5f,
                        new Vector2(.5f, 0.1f)), "",
                    VanillaSprites.BlueInsertPanelRound, null, 65);

                var tokenPanel = panel.AddPanel(new Info("TokenPanel",
                    1700, 150F * 1.5f, new Vector2(.5f, 0.1f)), null, RectTransform.Axis.Horizontal, 100);
                tokenPanel.AddText(new Info("ChannelLabel", 200, 150F * 1.5f), "Token: ", 65);
                tokenInput = tokenPanel
                    .AddInputField(new Info("TokenInput",
                            1700, 150F * 1.5f, new Vector2(.5f, 0.1f)),
                        "", VanillaSprites.BlueInsertPanelRound, null, 65
                    );
                tokenInput.InputField.contentType = TMP_InputField.ContentType.Password;

                TaskScheduler.ScheduleTask(() =>
                {
                    p.GetFirstActivePopup().bodyObj.transform.localPosition = new Vector3(0, 100, 0);
                });
            }, () => p.GetFirstActivePopup()?.bodyObj is not null);
        });
    }
}