using System;
using System.IO;
using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;

namespace StreamActions;

public class Settings : ModSettings
{
    public static ModSettingBool SaveToken { get; } = new(false)
    {
        displayName = nameof(SaveToken),
    };
}