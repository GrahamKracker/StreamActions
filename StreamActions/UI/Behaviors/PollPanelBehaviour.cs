using System;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Unity.Menu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Utils;
using Il2CppInterop.Runtime;
using Il2CppTMPro;
using MelonLoader.ICSharpCode.SharpZipLib;
using StreamActions.Actions;
using StreamActions.Actions.TimedActions;
using StreamActions.StreamingPlatforms;
using StreamActions.UI.Menus;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using Random = UnityEngine.Random;

namespace StreamActions.UI.Behaviors;

[RegisterTypeInIl2Cpp(false)]
public class PollPanelBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
    private float _timeUntilNextPoll;
    private float _lastVoteUpdate;
    private const float VoteUpdateCooldown = .5f;
    private StreamAction? _currentAction;
    private float _timeSinceLastActionStart;

    private void Start()
    {
        RandomizeActionOptions();
        PollPanel.Update();
        _timeUntilNextPoll = Random.Range(30, 60);
    }

    private float GetDeltaTime()
    {
        if(TimeManager.gamePaused && Settings.PausePollsOnPause)
            return 0;
        return Settings.ScalePollCountDown ? Time.deltaTime : Time.unscaledDeltaTime;
    }

    private void Update()
    {
        _timeUntilNextPoll -= GetDeltaTime();
        _timeSinceLastActionStart += GetDeltaTime();
        PollPanel.UpdateTimeUntil(_timeUntilNextPoll);


        if (_currentAction is TimedAction { IsOngoing: true } timedAction)
        {
            timedAction.OnUpdate();
            if(timedAction.Duration == -1)
                throw new InvalidOperationException("Duration not set for timed action: " + timedAction.DisplayName);
            PollPanel.UpdateTimeLeft(timedAction.Duration - _timeSinceLastActionStart);
            if (_timeSinceLastActionStart > timedAction.Duration)
            {
                timedAction.OnEnd();
                timedAction.IsOngoing = false;
                _currentAction = null;
                _timeSinceLastActionStart = 0;
                PollPanel.SetTimeLeftActive(false);
            }
        }

        if (_timeUntilNextPoll <= 0)
        {
            if (ActionOptions.TryGetValue(Votes.MaxBy(y => y.Value).Key, out _currentAction))
            {
                try
                {
                    _currentAction.OnChosen();
                    _timeSinceLastActionStart = 0;
                    MelonLogger.Msg("Activated action: " + _currentAction.ChoiceText);
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

                RandomizeActionOptions();
                PollPanel.Update();

                _timeUntilNextPoll = Random.Range(30, 60);
                if (_currentAction is TimedAction newTimedAction)
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

    private void RandomizeActionOptions()
    {
        ActionOptions.Clear();
        int i = 1;
        while (i <= 4)
        {
            var num = StreamAction.Random.Next(0, StreamAction.TotalWeight);
            var action = StreamAction.Weights.First(x => num >= x.Key.Item1 && num < x.Key.Item2).Value;
            if (ActionOptions.ContainsValue(action) || _currentAction == action) //todo: poor patch to alleviate bad design choices leading to data leaking over multiple polls
                                                                                 //todo: in the future, refactor to use new instances of each action instead, although weighted list may need to be overhauled due to reliance on mod helper's singleton modcontent system
            {
                continue;
            }

            try
            {
                action.BeforeVoting(StreamAction.Random);
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                continue;
            }

            ActionOptions[i] = action;
            i++;
        }
    }
}