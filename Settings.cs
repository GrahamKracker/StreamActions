using System;
using System.IO;
using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;

namespace StreamActions;

public class Settings : ModSettings
{
    public static ModSettingBool SaveTokenToFile { get; } = new(false)
    {
        onValueChanged = save =>
        {
            if(save)
                File.WriteAllLines(Path.Combine(CacheFolder, "auth.txt"), [
                    client.ConnectionCredentials.TwitchOAuth
                ]);
            else
            {
                File.Delete(Path.Combine(CacheFolder, "auth.txt"));
            }
        }
    };
}