using System;
using System.Collections.Generic;
using BTD_Mod_Helper.Api;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

namespace StreamActions;

[HarmonyPatch]
public abstract class StreamAction : NamedModContent
{
    public abstract void OnChosen();

    protected abstract Rarity Weight { get; }

    public abstract string ChoiceText { get; }
    protected abstract bool? IsPositiveEffect { get; }

    public virtual Color ChoiceColor
    {
        get {
            if (IsPositiveEffect == null)
            {
                return Color.cyan;
            }

            return (bool) IsPositiveEffect ? Color.green : Color.red;}
    }

    private static int TotalWeight { get; set; }

    protected static Random Random { get; } = new();

    private static readonly Dictionary<Tuple<int, int>, StreamAction> Weights = new();

    protected abstract void BeforeVoting(Random rand);

    /// <inheritdoc />
    public override void Register()
    {
        Weights.Add(new Tuple<int, int>(TotalWeight, TotalWeight += (int) Weight), this);
    }

    public static void RandomizeActionOptions()
    {
        ActionOptions.Clear();
        int i = 1;
        while(i <= 4)
        {
            var num = Random.Next(0, TotalWeight);
            var action = Weights.First(x => num >= x.Key.Item1 && num < x.Key.Item2).Value;
            if (ActionOptions.ContainsValue(action))
            {
                continue;
            }

            try
            {
                action.BeforeVoting(Random);
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

    protected enum Rarity
    {
        Common = 1000,
        Rare = 250,
        Epic = 100,
        Legendary = 50,
    }
}