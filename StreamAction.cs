using System;
using System.Collections.Generic;
using BTD_Mod_Helper.Api;
using UnityEngine;
using Random = System.Random;

namespace StreamActions;

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

            return (bool)IsPositiveEffect ? Color.green : Color.red;}
    }

    private static int TotalWeight { get; set; }

    private static Random Random { get; } = new();

    private static readonly Dictionary<Tuple<int, int>, StreamAction> Weights = new();

    protected abstract void BeforeSelection(Random rand);

    /// <inheritdoc />
    public override void Register()
    {
        Weights.Add(new Tuple<int, int>(TotalWeight, TotalWeight += (int) Weight), this);
        MelonLogger.Msg("Added " + Name + " to weights");
    }

    public static StreamAction[] GetRandomActionSelections()
    {
        var actions = new StreamAction[4];

        for (int i = 0; i < 4; i++)
        {
            var num = Random.Next(0, TotalWeight);
            var action = Weights.First(x => num >= x.Key.Item1 && num < x.Key.Item2).Value;
            if (actions.Contains(action))
            {
                i--;
                continue;
            }
            action.BeforeSelection(Random);
            actions[i] = action;
        }

        return actions;
    }

    protected enum Rarity
    {
        Common = 1000,
        Rare = 250,
        Epic = 100,
        Legendary = 50,
    }
}