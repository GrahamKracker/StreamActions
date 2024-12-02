using System;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.Menu;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppTMPro;
using StreamActions.StreamingPlatforms;
using StreamActions.UI.Menus;
using UnityEngine;

namespace StreamActions.UI.Behaviors;

[RegisterTypeInIl2Cpp(false)]
public class PlatformSelectionPanel(IntPtr ptr) : ModHelperComponent(ptr)
{

    private ModHelperButton MainButton => GetDescendent<ModHelperButton>("MainButton");
    private ModHelperImage Icon => GetDescendent<ModHelperImage>("Icon");
    private ModHelperText Name => GetDescendent<ModHelperText>("Name");

    public static PlatformSelectionPanel CreateForPlatform(StreamingPlatform platform, PlatformSetupMenu platformSetupMenu)
    {
        var platformSelectionPanel = CreateNew();
        platformSelectionPanel.Setup(platform, platformSetupMenu);
        return platformSelectionPanel;
    }

    [HideFromIl2Cpp]
    private void Setup(StreamingPlatform platform, PlatformSetupMenu platformSetupMenu)
    {
        MainButton.Button.SetOnClick(() =>
        {
            platformSetupMenu.SetSelectedPlatform(platform);
            MenuManager.instance.buttonClick2Sound.Play("ClickSounds");
        });

        Name.SetText(platform.DisplayName);


        Icon.Image.SetSprite(platform.IconReference);

        SetActive(true);
    }

    private static PlatformSelectionPanel CreateNew()
    {
        var info = new Info("PlatformTemplate")
        {
            Height = 200,
            FlexWidth = 1
        };

        var platform = new GameObject(info.Name, Il2CppType.Of<RectTransform>()).AddComponent<PlatformSelectionPanel>();
        platform.initialInfo = info;
        info.Apply(platform);

        var panel = platform.AddButton(new Info("MainButton", InfoPreset.FillParent), VanillaSprites.MainBGPanelBlue, null);

        panel.AddImage(new Info("Icon", 100, 0, 250, new Vector2(0, 0.5f)),
            VanillaSprites.UiGradient);

        panel.AddText(new Info("Name", 1000, 150, new Vector2(.5f, 0.5f)), "Name",
            69, TextAlignmentOptions.Center);


        platform.SetActive(true);

        return platform;
    }
}