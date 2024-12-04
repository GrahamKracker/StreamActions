using System;
using System.IO;
using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;

namespace StreamActions;

public class Settings : ModSettings
{
    public static ModSettingBool SaveToCache { get; } = new(false)
    {
        displayName = nameof(SaveToCache).Spaced(),
        description = "Save the twitch access token or youtube video id to a cache file and automatically connect using that token on boot, prioritizing saved twitch tokens over youtube video ids.",
    };

    public static ModSettingBool ScalePollCountDown { get; } = new(false)
    {
        displayName = nameof(ScalePollCountDown).Spaced(),
        description = "Whether the countdown until the next poll should be scaled with the in-game speed.",
    };

    public static ModSettingBool PausePollsOnPause { get; } = new(true)
    {
        displayName = nameof(PausePollsOnPause).Spaced(),
        description = "Whether to pause the poll countdown when the game is paused.",
    };
}