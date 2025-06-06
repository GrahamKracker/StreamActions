﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BTD_Mod_Helper.Api;
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
using StreamActions.Actions;
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
[assembly: MelonOptionalDependencies("WebAnalyticsLib")]

namespace StreamActions;

[HarmonyPatch]
public class Main : BloonsTD6Mod
{
    internal static MelonLogger.Instance ModLogger;

    private static readonly string ModFolder = Path.Combine(MelonEnvironment.ModsDirectory, "StreamActions");
    public static readonly string CacheFolder = Path.Combine(ModFolder, "Cache");

    public override void OnLateInitializeMelon()
    {
        ModLogger = LoggerInstance;
        if(!Directory.Exists(CacheFolder))
            Directory.CreateDirectory(CacheFolder);

        AppDomain.CurrentDomain.ProcessExit += (_, _) => OnApplicationQuit();
        AppDomain.CurrentDomain.UnhandledException += (_, _) => OnApplicationQuit();
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
        if (Settings.SaveToCache && File.Exists(Path.Combine(CacheFolder, "LastPlatform.txt")))
        {
            var lastPlatform = File.ReadAllText(Path.Combine(CacheFolder, "LastPlatform.txt"));
            var platform = StreamingPlatform.Platforms.Find(p => p.Name == lastPlatform);
            if(platform != null && platform.LoadFromCache())
            {
                ModLogger.Msg($"Successfully loaded {platform.DisplayName} from cache");
                StreamingPlatform.CurrentPlatform = platform;
            }
        }
    }

    public static void ChatMessageReceived(string chatMessage)
    {
        if (Settings.DebugMode)
            ModLogger.Msg("Chat message received: " + chatMessage);
        int charPos = 0;
        char cToCheck = chatMessage[0];
        if (cToCheck is not '1' and not '2' and not '3' and not '4')
        {
            return;
        }
        while (charPos < chatMessage.Length)
        {
            if (chatMessage[charPos] != cToCheck)
            {
                return;
            }
            charPos++;
        }

        if (int.TryParse(chatMessage[0].ToString(), out int i) && i is >= 1 and <= 4)
        {
            Votes[i]++;
            MelonLogger.Msg("Vote received for " + i);
        }
    }

    public static Dictionary<int, StreamAction> ActionOptions { get; } = new();

    public static Dictionary<int, int> Votes { get; } = new()
    {
        {1, 0},
        {2, 0},
        {3, 0},
        {4, 0},
    };

    [HarmonyPatch(typeof(MainHudLeftAlign), nameof(MainHudLeftAlign.Initialise))]
    [HarmonyPostfix]
    private static void MainHudLeftAlign_Initialise(MainHudLeftAlign __instance)
    {
        PollPanel.Create(__instance);
    }

    /// <inheritdoc />
    public override void OnApplicationQuit()
    {
        ModContent.GetInstance<YoutubePlatform>().KillSeleniumWrapper();
    }
}