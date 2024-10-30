using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using Il2CppAssets.Scripts.Unity.UI_New.Main;
using Il2CppAssets.Scripts.Unity.UI_New.Main.Home;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using Il2CppTMPro;
using StreamActions.UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace StreamActions.UI;

public static class MainMenuButton
{
    private static SpriteReference Sprite => ModContent.GetSpriteReference<Main>("TwitchPlatform-Icon");

    public static void Create(MainMenu mainMenu)
    {
        var mainMenuTransform = mainMenu.transform.Cast<RectTransform>();
        var bottomGroup = mainMenuTransform.FindChild("BottomButtonGroup");
        var baseButton = bottomGroup.FindChild("Monkeys").gameObject;
        var copyLocalFrom = bottomGroup.FindChild("Knowledge").gameObject;

        bottomGroup.TranslateScaled(new Vector3(-300, 0, 0));


        var mainMenuButton = baseButton.Duplicate(bottomGroup);
        mainMenuButton.name = "StreamAction";
        mainMenuButton.transform.localPosition = new Vector3(-1600, 0, 0);
        mainMenuButton.RemoveComponent<PipEventChecker>();
        mainMenuButton.GetComponentInChildrenByName<Image>("Button").SetSprite(Sprite);
        var text = mainMenuButton.GetComponentInChildren<NK_TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.BottomGeoAligned;
        text.localizeKey = "Stream \n Sign-In";

        mainMenuButton.GetComponentInChildren<Button>().SetOnClick(() => ModGameMenu.Open<PlatformSetupMenu>());

        mainMenuButton.transform.SetAsFirstSibling();

        mainMenuButton.GetComponentInChildrenByName<RectTransform>("ParagonAvailable").gameObject.SetActive(false);


        var matchLocalPosition = mainMenuButton.transform.GetChild(0).gameObject.AddComponent<MatchLocalPosition>();
        matchLocalPosition.transformToCopy = copyLocalFrom.transform.GetChild(0);


        var rect = mainMenuTransform.rect;
        var aspectRatio = rect.width / rect.height;

        if (aspectRatio < 1.5)
        {
            matchLocalPosition.offset = new Vector3(560, 0);
            matchLocalPosition.scale = new Vector3(1, 3.33f, 1);
        }
        else if (aspectRatio < 1.7)
        {
            matchLocalPosition.offset = new Vector3(450, 0);
            matchLocalPosition.scale = new Vector3(1, 3f, 1);
        }
        else
        {
            matchLocalPosition.offset = new Vector3(50, 0);
        }
    }
}