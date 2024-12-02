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
    private StreamAction? currentAction;
    private float _timeSinceLastActionStart;

    private void Start()
    {
        StreamAction.RandomizeActionOptions();
        PollPanel.Update();
        _timeUntilNextPoll = Random.Range(30, 60);
    }

    private void Update()
    {
        _timeUntilNextPoll -= Settings.ScalePollCountDown ? Time.deltaTime : Time.unscaledDeltaTime;
        _timeSinceLastActionStart += Settings.ScalePollCountDown ? Time.deltaTime : Time.unscaledDeltaTime;
        PollPanel.UpdateTimeUntil(_timeUntilNextPoll);


        var timedAction = currentAction as TimedAction;
        if (timedAction is { IsOngoing: true })
        {
            timedAction.OnUpdate();
            PollPanel.UpdateTimeLeft(timedAction.Duration - _timeSinceLastActionStart, timedAction);
            if (_timeSinceLastActionStart > timedAction.Duration)
            {
                timedAction.OnEnd();
                timedAction.IsOngoing = false;
                currentAction = null;
                _timeSinceLastActionStart = 0;
                PollPanel.SetTimeLeftActive(false);
            }
        }




        if (_timeUntilNextPoll <= 0)
        {
            if (ActionOptions.TryGetValue(Votes.MaxBy(y => y.Value).Key, out currentAction))
            {
                try
                {
                    currentAction.OnChosen();
                    _timeSinceLastActionStart = 0;
                    MelonLogger.Msg("Activated action: " + currentAction.ChoiceText);
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
                if (currentAction is TimedAction newTimedAction)
                {
                    _timeUntilNextPoll = Math.Max(_timeUntilNextPoll, newTimedAction.Duration);
                    newTimedAction.IsOngoing = true;
                    PollPanel.SetTimeLeftActive(true);
                }
            }
        }

        if (Time.time - _lastVoteUpdate < VoteUpdateCooldown)
        {
            PollPanel.UpdateVotes();
            _lastVoteUpdate = Time.time;
        }
    }
}