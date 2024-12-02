using System;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;

namespace StreamActions.StreamingPlatforms;

public class YoutubePlatform : StreamingPlatform
{
    public override async void ConnectToPlatform()
    {
        try
        {
            var url = "https://www.youtube.com/live_chat?v=FNGAvwzgKf4";


        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    private void OnMessageReceived(string author, string message)
    {
        if (ChannelsAnswered.Add(author))
            ChatMessageReceived(message);
    }

    private void ShowConnectPopup()
    {
        //todo
        //if (_driver is null)
            ConnectToPlatform();
        //GetMessages();
    }

    /// <inheritdoc />
    protected override string[] CacheData => [];

    /// <inheritdoc />
    protected override bool LoadFromCacheData(string[] lines)
    {
        return false;
    }

    /// <inheritdoc />
    public override void CreateConnectPanel(ModHelperPanel rightMenu)
    {
        const float height = 200;
        const float labelWidth = 300;
        const float inputWidth = 1200;


        rightMenu.AddButton(new Info("ConnectButton", 600, 250), VanillaSprites.GreenBtnLong,
            new System.Action(ShowConnectPopup)).AddText(new Info("Text", InfoPreset.FillParent), "Connect", 69);
    }
}