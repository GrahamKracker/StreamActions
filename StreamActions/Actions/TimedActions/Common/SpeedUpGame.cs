﻿using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Utils;
using UnityEngine;
using Random = System.Random;

namespace StreamActions.Actions.TimedActions;

public class SpeedUpGame : TimedAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        _speedUpTimeScale = Speed;
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Common;

    /// <inheritdoc />
    public override string ChoiceText => $"Lock the game speed to {Speed:N2}x for {Duration}s";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {
        Speed = (float)(rand.NextDouble() * 4 + 1);
        Duration = rand.Next(15, 35);
    }

    private float Speed { get; set; }

    private static float _speedUpTimeScale = 1;

    [HarmonyPatch(typeof(TimeManager), nameof(TimeManager.FastForwardTimeScale), MethodType.Getter)]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    private static bool Prefix(ref double __result)
    {
        if (!IsActive<SpeedUpGame>())
        {
            return true;
        }

        __result = _speedUpTimeScale;
        return false;
    }

    [HarmonyPatch(typeof(TimeManager), nameof(TimeManager.Update))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    private static void Postfix()
    {
        if (InGame.Bridge == null || InGame.instance == null || !IsActive<SpeedUpGame>())
            return;
        Time.timeScale = TimeManager.gamePaused ? 0 : _speedUpTimeScale;
    }
}