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

    public static ModSettingBool ScalePollCountDown { get; } = new(true)
    {
        displayName = nameof(ScalePollCountDown).Spaced(),
        description = "Whether the countdown until the next poll should be scaled with the in-game speed. If enabled, makes the timer pause counting down when the game is paused as well.",
    };
}