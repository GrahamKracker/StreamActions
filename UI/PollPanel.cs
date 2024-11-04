using System;
using System.Globalization;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppTMPro;
using StreamActions.UI.Behaviors;
using UnityEngine;

namespace StreamActions.UI;

public static class PollPanel
{
    private static readonly ModHelperText[] Texts = new ModHelperText[4];
    private static ModHelperText? _timeText;
    public static void Create(MainHudLeftAlign hudLeftAlign)
    {
        const int textHeight = 100;
        const int textWidth = 1500;
        var panel = hudLeftAlign.cashButton.transform.parent.parent.gameObject.AddModHelperPanel(new Info("PollPanel")
        {
            Position = new Vector2(850, -1800),
            Width = 0,
            Height = textHeight*6,
        }, null, RectTransform.Axis.Vertical, 25);
        panel.gameObject.AddComponent<PollPanelBehaviour>();

        _timeText = panel.AddText(new Info("TimeUntil", 0, 0, textWidth, textHeight), "TimeUntil", 42,
                TextAlignmentOptions.MidlineLeft);

        _timeText.Text.enableAutoSizing = true;
        _timeText.Text.color = Color.yellow;

        (Texts[0] = panel.AddText(new Info("Option1", 0, 0, textWidth, textHeight), "1", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        (Texts[1] = panel.AddText(new Info("Option2", 0, 0, textWidth, textHeight), "2", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        (Texts[2] = panel.AddText(new Info("Option3", 0, 0, textWidth, textHeight), "3", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        (Texts[3] = panel.AddText(new Info("Option4", 0, 0, textWidth, textHeight), "4", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        var advertising = panel.AddText(new Info("Advertising", 0, 0, textWidth, textHeight), "Made by GrahamKracker", 42, TextAlignmentOptions.MidlineLeft);
        advertising.Text.enableAutoSizing = true;
        advertising.Text.color = new Color(117, 0, 195);
    }

    private static bool DoTextsExist() => !Texts.Any(x => x == null);

    public static void Update()
    {
        if(!DoTextsExist()) return;


        int i = 0;
        foreach (var streamAction in Main.ActionOptions.Values)
        {
            Texts[i].SetText($"{i + 1}:  {streamAction.ChoiceText} ({Votes[i + 1]} votes)");
            Texts[i].Text.color = streamAction.ChoiceColor;
            i++;
        }
    }

    public static void UpdateVotes()
    {
        if (Texts.Any(x => x == null))
            return;

        foreach ((int index, int amount) in Main.Votes)
        {
            int i = index - 1;
            var text = Texts[i].Text.text;
            var voteIndex = text.LastIndexOf(" (", StringComparison.Ordinal);
            if (voteIndex == -1)
            {
                Texts[i].SetText($"{text} ({amount} votes)");
                continue;
            }
            Texts[i].SetText($"{text.Remove(voteIndex)} ({amount} votes)");
        }
    }

    public static void UpdateTimeUntil(float timeUntil)
    {
        if (_timeText != null)
            _timeText.SetText($"{Math.Floor(timeUntil)}s left of voting");
    }
}