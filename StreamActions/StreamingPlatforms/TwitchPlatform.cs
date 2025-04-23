using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppTMPro;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using UnityEngine;
using Action = Il2CppSystem.Action;
using Random = UnityEngine.Random;

namespace StreamActions.StreamingPlatforms;

public class TwitchPlatform : StreamingPlatform
{
    private TwitchClient? client;
    private ModHelperInputField ChannelInput { get; set; } = null!;

    /// <inheritdoc />
    public override int Priority => -1;

    /// <inheritdoc />
    public override void ConnectToPlatform()
    {
        InitClient(ChannelInput.InputField.text);
    }

    private void InitClient(string channel)
    {
        try
        {
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            client = new TwitchClient(new WebSocketClient(clientOptions));
            client.Initialize(new ConnectionCredentials("justinfan" + Random.RandomRangeInt(1,10000), ""),
                channel);

            client.OnMessageReceived += Client_OnMessageReceived;

            Task.Run(() =>

            {
                client.Connect();
                if (Settings.SaveToCache)
                {
                    SaveToCache();
                }

                PopupScreen.instance.SafelyQueue(p => { p.ShowOkPopup("Connected to Twitch."); });
            });
        }
        catch (Exception e)
        {
            MelonLogger.Error(e);
        }
    }

    /// <inheritdoc />
    protected override string[] CacheData => [client.JoinedChannels[0].Channel];

    /// <inheritdoc />
    protected override bool LoadFromCacheData(string[] lines)
    {
        InitClient(lines[0]);
        return true;
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        OnMessageReceived(e.ChatMessage.UserId, e.ChatMessage.Message);
    }

    /// <inheritdoc />
    public override void CreateConnectPanel(ModHelperPanel rightMenu)
    {
        const float height = 200;
        const float labelWidth = 300;

        var channelPanel = rightMenu.AddPanel(new Info("ChannelPanel")
        {
            Width = rightMenu.initialInfo.Width - 50 * 2,
            Height = 200,
        }, null, RectTransform.Axis.Horizontal, 100);
        channelPanel.AddText(new Info("ChannelLabel", labelWidth, height), "Channel: ", 65).Text.enableAutoSizing =
            true;
        ChannelInput = channelPanel.AddInputField(new Info("TokenInput")
            {
                FlexWidth = 1,
                Height = height,
            }, "",
            VanillaSprites.BlueInsertPanelRound, null, 65);
    }

    /// <inheritdoc />
    public override void Disconnect()
    {
        client?.Disconnect();
    }
}