using System;
using System.Collections.Generic;
using System.IO;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using Il2CppNinjaKiwi.Common.ResourceUtils;

namespace StreamActions.StreamingPlatforms;
//todo https://github.com/Agash/YTLiveChat for youtube


public abstract class StreamingPlatform : NamedModContent
{
    public static List<StreamingPlatform> Platforms { get; } = [];
    protected HashSet<string> ChannelsAnswered { get; } = [];

    public virtual void OnNewPoll(){
        ChannelsAnswered.Clear();
    }
    public abstract void ConnectToPlatform();

    protected abstract string[] CacheData { get; }

    protected void SaveToCache()
    {
        var lines = new List<string> { Name };
        lines.AddRange(CacheData);
        if(Settings.SaveToken)
            File.WriteAllLines(CacheFile, lines);
    }

    protected abstract bool LoadFromCacheData(string[] lines);

    public bool LoadFromCache()
    {
        string[] lines;
        if(Settings.SaveToken && File.Exists(CacheFile) && (lines = File.ReadAllLines(CacheFile)).Any() && lines[0] == Name)
            return LoadFromCacheData(File.ReadAllLines(CacheFile));
        return false;
    }
    public abstract void CreateConnectPanel(ModHelperPanel rightMenu);

    protected virtual string Icon => GetType().Name + "-Icon";

    /// <summary>
    /// If you're not going to use a custom .png for your Icon, use this to directly control its SpriteReference
    /// </summary>
    public virtual SpriteReference IconReference => this.GetSpriteReferenceOrDefault(this.Icon);

    /// <inheritdoc />
    public override void Register()
    {
        Platforms.Add(this);
    }
}