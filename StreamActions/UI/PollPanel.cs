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
    //todo: add an option to have this in a separate window so streamers cant see the next option, probably using winapi (fun) to make a new window handler, could also use winforms

    private static readonly ModHelperText[] OptionTexts = new ModHelperText[4];
    private static ModHelperText? _timeText;
    private static ModHelperText? _timeLeftText;
    public static void Create(MainHudLeftAlign hudLeftAlign)
    {
        const int textHeight = 100;
        const int textWidth = 2500;
        var panel = hudLeftAlign.cashButton.transform.parent.parent.gameObject.AddModHelperPanel(new Info("PollPanel")
        {
            Position = new Vector2(textWidth/2f, -1400)
        }, VanillaSprites.BlueInsertPanelRound, RectTransform.Axis.Vertical, 25);
        panel.gameObject.AddComponent<PollPanelBehaviour>();


        _timeText = panel.AddText(new Info("TimeUntil", 0, 0, textWidth, textHeight), "TimeUntil", 42,
                TextAlignmentOptions.MidlineLeft);

        _timeText.Text.enableAutoSizing = true;
        _timeText.Text.color = Color.yellow;
        _timeText.Text.raycastTarget = false;

        (OptionTexts[0] = panel.AddText(new Info("Option1", 0, 0, textWidth, textHeight), "1", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        (OptionTexts[1] = panel.AddText(new Info("Option2", 0, 0, textWidth, textHeight), "2", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        (OptionTexts[2] = panel.AddText(new Info("Option3", 0, 0, textWidth, textHeight), "3", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        (OptionTexts[3] = panel.AddText(new Info("Option4", 0, 0, textWidth, textHeight), "4", 42, TextAlignmentOptions.MidlineLeft)).Text
            .enableAutoSizing = true;
        var advertising = panel.AddText(new Info("Advertising", 0, 0, textWidth, textHeight), "Made by GrahamKracker", 42, TextAlignmentOptions.MidlineLeft);
        advertising.Text.enableAutoSizing = true;
        advertising.Text.color = new Color(117, 0, 195);
        advertising.Text.raycastTarget = false;

        _timeLeftText = panel.AddText(new Info("TimeLeft", 0, 0, textWidth, textHeight), "", 42,
            TextAlignmentOptions.MidlineLeft);
        _timeLeftText.Text.enableAutoSizing = true;
        _timeLeftText.Text.color = Color.white;
        _timeLeftText.SetActive(false);
        _timeLeftText.Text.raycastTarget = false;
        panel.Background.raycastTarget = false;

        foreach (var text in OptionTexts)
        {
            text.Text.raycastTarget = false;
        }
    }

    private static bool DoTextsExist() => !OptionTexts.Any(x => x == null);

    public static void Update()
    {
        if(!DoTextsExist()) return;

        int i = 0;
        foreach (var streamAction in ActionOptions.Values)
        {
            OptionTexts[i].SetText($"{i + 1}:  {streamAction.ChoiceText} ({Votes[i + 1]} votes)");
            OptionTexts[i].Text.color = streamAction.ChoiceColor;
            i++;
        }
    }

    public static void UpdateVotes()
    {
        if (OptionTexts.Any(x => x == null))
            return;

        foreach ((int index, int amount) in Votes)
        {
            int i = index - 1;
            var text = OptionTexts[i].Text.text;
            var voteIndex = text.LastIndexOf(" (", StringComparison.Ordinal);
            if (voteIndex == -1)
            {
                OptionTexts[i].SetText($"{text} ({amount} votes)");
                continue;
            }
            OptionTexts[i].SetText($"{text.Remove(voteIndex)} ({amount} votes)");
        }
    }

    public static void UpdateTimeUntil(float timeUntil)
    {
        if (_timeText != null)
            _timeText.SetText($"{Math.Floor(timeUntil)}s left of voting");
    }

    public static void UpdateTimeLeft(float timeLeft)
    {
        if (_timeLeftText != null)
        {
            _timeLeftText.SetText($"{Math.Floor(timeLeft)}s left of previous action");
        }
    }

    public static void SetTimeLeftActive(bool active) => _timeLeftText?.SetActive(active);
}