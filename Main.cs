using System;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.UI_New.Main;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppTMPro;
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

    public static TwitchClient? client;

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

    public void ConnectToTwitch(ConnectionCredentials credentials)
    {
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };

        client = new TwitchClient(new WebSocketClient(clientOptions));
        client.Initialize(credentials, "channel");


        client.OnMessageReceived += Client_OnMessageReceived;

        client.Connect();
        MelonLogger.Msg("connected");
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        MelonLogger.Msg("received chat message:");
        MelonLogger.Msg(e.ChatMessage.Message);
    }

    public void CreateSignInPopup()
    {
        PopupScreen.instance.SafelyQueue(p =>
        {
            ModHelperInputField tokenInput = null!;

            p.ShowPopup(PopupScreen.Placement.menuCenter,
                "Sign in to Twitch","Go to https://twitchapps.com/tmi/ to get your token:",
                new Action(() =>
                {
                    ConnectToTwitch(new ConnectionCredentials("streamactions_bot", tokenInput.CurrentValue));
                }), "Confirm", null, "Cancel", Popup.TransitionAnim.Scale);

            TaskScheduler.ScheduleTask(() =>
            {
                var panel = p.GetFirstActivePopup().bodyObj.AddModHelperPanel(new Info("StreamActionPanel", 400, 650));

                tokenInput = panel
                    .AddInputField(new Info("TokenInput",
                            421.5F * 1.5f, 150F * 1.5f, new Vector2(.5f, 0.1f)),
                        "", VanillaSprites.BlueInsertPanelRound, null, 52
                    );
                tokenInput.InputField.contentType = TMP_InputField.ContentType.Password;

                TaskScheduler.ScheduleTask(() =>
                {
                    p.GetFirstActivePopup().bodyObj.transform.localPosition = new Vector3(0, 50, 0);
                });
            }, () => p.GetFirstActivePopup()?.bodyObj is not null);
        });
    }
}