using System;
using System.Globalization;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppTMPro;
using UnityEngine;

namespace StreamActions.UI;

public static class SelectionPanel
{
    private static readonly ModHelperText[] Texts = new ModHelperText[4];
    private static ModHelperText? _timeText;
    public static void Create(MainHudLeftAlign hudLeftAlign)
    {
        const int textHeight = 100;
        var panel = hudLeftAlign.cashButton.transform.parent.parent.gameObject.AddModHelperPanel(new Info("ChoicesPanel")
        {
            Anchor = new Vector2(.5f, .5f),
            Pivot = new Vector2(.5f, .5f),
            Position = new Vector2(600, -2000),
            Width = 1000,
            Height = textHeight*6,
        }, null, RectTransform.Axis.Vertical, 0);
        _timeText = panel.AddText(new Info("TimeUntil", 0, 0, 1000, textHeight), "TimeUntil", 42,
                TextAlignmentOptions.MidlineLeft);

        _timeText.Text.enableAutoSizing = true;
        _timeText.Text.color = Color.yellow;

        (Texts[0] = panel.AddText(new Info("Option1", 0, 0, 1000, textHeight), "1", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        (Texts[1] = panel.AddText(new Info("Option2", 0, 0, 1000, textHeight), "2", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        (Texts[2] = panel.AddText(new Info("Option3", 0, 0, 1000, textHeight), "3", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        (Texts[3] = panel.AddText(new Info("Option4", 0, 0, 1000, textHeight), "4", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        var advertising = panel.AddText(new Info("Advertising", 0, 0, 1000, textHeight), "Made by GrahamKracker", 42, TextAlignmentOptions.MidlineLeft);
        advertising.Text.enableAutoSizing = true;
        advertising.Text.color = new Color(117, 0, 195);
    }

    public static bool Update(StreamAction[] streamActions)
    {
        if(Texts.Any(x=>x == null))
            return false;

        for (var i = 0; i < streamActions.Length; i++)
        {
            var streamAction = streamActions[i];

            Texts[i].SetText($"{i + 1}: {streamAction.ChoiceText}");
            Texts[i].Text.color = streamAction.ChoiceColor;
        }

        return true;
    }

    public static void UpdateTimeUntil(float timeUntil)
    {
        if (_timeText != null)
            _timeText.SetText($"{Math.Floor(timeUntil)}s left of voting");
    }
}