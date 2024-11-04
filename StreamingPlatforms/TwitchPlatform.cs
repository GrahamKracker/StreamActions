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

namespace StreamActions.StreamingPlatforms;

public class TwitchPlatform : StreamingPlatform
{
    private TwitchClient? client;
    private ModHelperInputField TokenInput { get; set; } = null!;
    private ModHelperInputField ChannelInput { get; set; } = null!;

    /// <inheritdoc />
    public override void ConnectToPlatform()
    {
        InitClient(ChannelInput.InputField.text, TokenInput.InputField.text);
    }

    private void InitClient(string username, string token)
    {
        try
        {
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            client = new TwitchClient(new WebSocketClient(clientOptions));
            client.Initialize(new ConnectionCredentials(username, token),
                username);

            client.OnMessageReceived += Client_OnMessageReceived;

            Task.Run(() =>

            {
                client.Connect();
                if (Settings.SaveToken)
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
    protected override string[] CacheData => [client.ConnectionCredentials.TwitchUsername, client.ConnectionCredentials.TwitchOAuth];

    /// <inheritdoc />
    protected override bool LoadFromCacheData(string[] lines)
    {
        InitClient(lines[1], lines[2]);
        return true;
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        if(ChannelsAnswered.Add(e.ChatMessage.Channel))
            ChatMessageReceived(e.ChatMessage.Message);
    }

    /// <inheritdoc />
    public override void CreateConnectPanel(ModHelperPanel rightMenu)
    {
        const float height = 200;
        const float labelWidth = 300;
        const float inputWidth = 1200;

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

        var tokenPanel = rightMenu.AddPanel(new Info("TokenPanel")
        {
            Width = rightMenu.initialInfo.Width - 50 * 2,
            Height = 200,
        }, null, RectTransform.Axis.Horizontal, 100);

        tokenPanel.AddText(new Info("ChannelLabel", labelWidth, height), "Token: ", 65).Text.enableAutoSizing = true;
        TokenInput = tokenPanel

            .AddInputField(new Info("TokenInput")
                {
                    FlexWidth = 1,
                    Height = height,
                },
                "", VanillaSprites.BlueInsertPanelRound, null, 65
            );
        TokenInput.InputField.contentType = TMP_InputField.ContentType.Password;

        rightMenu.AddButton(new Info("ConnectButton", 600, 250), VanillaSprites.GreenBtnLong, new System.Action(ConnectToPlatform)).AddText(new Info("Text", InfoPreset.FillParent),"Connect", 69);
    }
}