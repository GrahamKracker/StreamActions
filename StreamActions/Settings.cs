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
        description = "Save the relevant data for a streaming platform to a cache file and automatically connects using that data to the last used streaming platform when the game is started.",
    };

    public static ModSettingBool ScalePollCountDown { get; } = new(false)
    {
        displayName = nameof(ScalePollCountDown).Spaced(),
        description = "Whether poll countdowns should be scaled with the in-game speed.",
    };

    public static ModSettingBool PausePollsOnGamePause { get; } = new(true)
    {
        displayName = nameof(PausePollsOnGamePause).Spaced(),
        description = "Whether to pause the poll countdown when the game is paused.",
    };
}