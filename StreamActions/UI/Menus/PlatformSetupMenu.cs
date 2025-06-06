using System;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.UI_New.ChallengeEditor;
using Il2CppInterop.Runtime;
using Il2CppTMPro;
using StreamActions.StreamingPlatforms;
using StreamActions.UI.Behaviors;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace StreamActions.UI.Menus;

public class PlatformSetupMenu : ModGameMenu<ExtraSettingsScreen>
{
    private const int Padding = 50;

    private const int MenuWidth = 3600;
    private const int MenuHeight = 1900;

    private const int LeftMenuWidth = 1750;
    private const int RightMenuWidth = 1750;

    private ModHelperScrollPanel LeftScrollPanel { get; set; } = null!;
    private CanvasGroup CanvasGroup { get; set; } = null!;
    private ModHelperPanel RightMenu { get; set; } = null!;


    public override void OnMenuUpdate()
    {
        if (Closing)
        {
            CanvasGroup.alpha -= .07f;
        }
    }

    public override bool OnMenuOpened(Object data)
    {
        GameMenu.anim.updateMode = AnimatorUpdateMode.UnscaledTime;

        CommonForegroundHeader.SetText("Streaming Platforms");
        CommonForegroundHeader.gameObject.SetActive(true);

        var panelTransform = GameMenu.gameObject.GetComponentInChildrenByName<RectTransform>("Panel");
        var panel = panelTransform.gameObject;
        panel.DestroyAllChildren();

        var modsMenu = GameMenu.gameObject.AddModHelperPanel(new Info("Platforms", MenuWidth, MenuHeight));
        CanvasGroup = modsMenu.AddComponent<CanvasGroup>();

        CreateLeftMenu(modsMenu);
        CreateRightMenu(modsMenu);

        return false;
    }



    private void CreateLeftMenu(ModHelperComponent modsMenu)
    {
        var leftMenu = modsMenu.AddPanel(
            new Info("LeftMenu", (MenuWidth - LeftMenuWidth) / -2f, 0, LeftMenuWidth, MenuHeight),
            VanillaSprites.MainBGPanelBlue, RectTransform.Axis.Vertical, Padding, Padding
        );

        LeftScrollPanel = leftMenu.AddScrollPanel(new Info("PlatformList", InfoPreset.Flex), RectTransform.Axis.Vertical,
            VanillaSprites.BlueInsertPanelRound, Padding, Padding);

        foreach (var streamingPlatform in StreamingPlatform.Platforms)
        {
            LeftScrollPanel.AddScrollContent(PlatformSelectionPanel.CreateForPlatform(streamingPlatform, this));
        }
    }

    public void SetSelectedPlatform(StreamingPlatform platform)
    {
        RightMenu.gameObject.DestroyAllChildren();
        platform.CreateConnectPanel(RightMenu);
        RightMenu.AddButton(new Info("ConnectButton", 600, 250), VanillaSprites.GreenBtnLong,
            new Action(() => ConnectToPlatform(platform))).AddText(new Info("Text", InfoPreset.FillParent), "Connect", 69);
    }

    private void ConnectToPlatform(StreamingPlatform platform)
    {
        var currentPlatform = StreamingPlatform.CurrentPlatform;
        if (currentPlatform != null)
        {
            ModLogger.Msg($"Disconnecting from {currentPlatform.DisplayName}...");
            currentPlatform.Disconnect();
        }

        ModLogger.Msg($"Connecting to {platform.DisplayName}...");
        StreamingPlatform.CurrentPlatform = platform;

        platform.ConnectToPlatform();
        ModLogger.Msg($"Connected to {platform.DisplayName}.");
    }

    private void CreateRightMenu(ModHelperPanel modPanel)
    {
        RightMenu = modPanel.AddPanel(
            new Info("SetupMenu", (MenuWidth - RightMenuWidth) / 2f, 0, RightMenuWidth, MenuHeight),
            VanillaSprites.MainBGPanelBlue, RectTransform.Axis.Vertical, 100, 150);
    }

}