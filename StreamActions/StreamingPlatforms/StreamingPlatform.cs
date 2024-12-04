using System;
using System.Collections.Generic;
using System.IO;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
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

    public virtual int Priority => 0;

    public abstract void ConnectToPlatform();

    protected abstract string[] CacheData { get; }

    protected void SaveToCache()
    {
        if(Settings.SaveToCache)
            File.WriteAllLines(CacheFile, CacheData);
    }

    protected virtual void OnMessageReceived(string author, string message)
    {
        if ((InGame.instance == null || !InGame.instance.IsInGame()) || ChannelsAnswered.Add(author))
            ChatMessageReceived(message);
    }

    protected virtual string CacheFile => Path.Combine(CacheFolder, Name + ".txt");

    protected abstract bool LoadFromCacheData(string[] lines);

    public bool LoadFromCache()
    {
        string[] lines;
        if(Settings.SaveToCache && File.Exists(CacheFile) && (lines = File.ReadAllLines(CacheFile)).Any())
            return LoadFromCacheData(lines);
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