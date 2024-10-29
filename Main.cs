using System;
using System.IO;
using System.Threading.Tasks;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Main;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppAssets.Scripts.Unity.UI_New.Utils;
using Il2CppTMPro;
using MelonLoader.Utils;
using StreamActions;
using StreamActions.StreamingPlatforms;
using StreamActions.UI;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

using TwitchLib.Client.Events;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TaskScheduler = BTD_Mod_Helper.Api.TaskScheduler;

[assembly: MelonInfo(typeof(StreamActions.Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace StreamActions;

[HarmonyPatch]
public class Main : BloonsTD6Mod
{
    internal static MelonLogger.Instance Logger;

    public static string ModFolder = Path.Combine(MelonEnvironment.ModsDirectory, "StreamActions");
    public static string CacheFolder = Path.Combine(ModFolder, "Cache");
    public static string CacheFile = Path.Combine(CacheFolder, "cachedlogin.txt");

    private static string TwitchUserName = "grahamkracker1";

    public override void OnInitialize()
    {
        Logger = LoggerInstance;
        if(!Directory.Exists(CacheFolder))
            Directory.CreateDirectory(CacheFolder);
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
        if (Settings.SaveToken)
        {
            foreach (var streamingPlatform in StreamingPlatform.Platforms)
            {
                if(streamingPlatform.LoadFromCache())
                    return;
            }
        }
    }

    public static void ChatMessageReceived(string chatMessage)
    {
        MelonLogger.Msg("received: " + chatMessage);
    }

    private static float TimeUntilNextAction;

    public override void OnUpdate()
    {
        TimeUntilNextAction -= Settings.ScalePollCountDown ? Time.deltaTime : Time.unscaledDeltaTime;
        SelectionPanel.UpdateTimeUntil(TimeUntilNextAction);

        if (InGame.instance != null && InGame.instance.IsInGame() && TimeUntilNextAction <= 0)
        {
            SelectionPanel.Update(StreamAction.GetRandomActionSelections());
            TimeUntilNextAction = Random.Range(30, 60);
        }
    }

    [HarmonyPatch(typeof(MainHudLeftAlign), nameof(MainHudLeftAlign.Initialise))]
    [HarmonyPostfix]
    private static void InGameUIRect_UpdateRect(MainHudLeftAlign __instance)
    {
        SelectionPanel.Create(__instance);
    }
}