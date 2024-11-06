using System;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.Menu;
using Il2CppInterop.Runtime;
using Il2CppTMPro;
using StreamActions.StreamingPlatforms;
using StreamActions.UI.Menus;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StreamActions.UI.Behaviors;

[RegisterTypeInIl2Cpp(false)]
public class PollPanelBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
    private float _timeUntilNextPoll;
    private float _lastVoteUpdate;
    private const float VoteUpdateCooldown = .5f;

    private void Start()
    {
        StreamAction.RandomizeActionOptions();
        PollPanel.Update();
        _timeUntilNextPoll = Random.Range(30, 60);
    }

    private void Update()
    {
        _timeUntilNextPoll -= Settings.ScalePollCountDown ? Time.deltaTime : Time.unscaledDeltaTime;
        PollPanel.UpdateTimeUntil(_timeUntilNextPoll);

        if (_timeUntilNextPoll <= 0 && ActionOptions.TryGetValue(Votes.MaxBy(y => y.Value).Key, out var chosen))
        {
            try
            {
                chosen.OnChosen();
                MelonLogger.Msg("Activated action: " + chosen.ChoiceText);
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
            }

            foreach (var i in Votes.Keys)
            {
                Votes[i] = 0;
            }

            foreach (var streamingPlatform in StreamingPlatform.Platforms)
            {
                streamingPlatform.OnNewPoll();
            }

            StreamAction.RandomizeActionOptions();
            PollPanel.Update();

            _timeUntilNextPoll = Random.Range(30, 60);
            if(chosen is TimedAction timedAction)
                _timeUntilNextPoll = Math.Max(_timeUntilNextPoll, timedAction.Duration);
        }

        if (Time.time - _lastVoteUpdate < VoteUpdateCooldown)
        {
            PollPanel.UpdateVotes();
            _lastVoteUpdate = Time.time;
        }
    }
}