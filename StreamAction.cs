using System;
using System.Collections.Generic;
using BTD_Mod_Helper.Api;

namespace StreamActions;

public abstract class StreamAction : NamedModContent
{
    public abstract void OnChosen();

    protected abstract Rarity Weight { get; }

    public abstract string ChoiceText { get; }

    private static int TotalWeight { get; set; }

    private static Random Random { get; } = new();

    private static readonly Dictionary<Tuple<int, int>, StreamAction> Weights = new();
    
    protected virtual void BeforeSelection(Random rand) { }

    /// <inheritdoc />
    public override void Register()
    {
        Weights.Add(new Tuple<int, int>(TotalWeight, TotalWeight + (int) Weight), this);
        TotalWeight += (int) Weight;
    }

    public static StreamAction[] GetRandomActionSelections()
    {
        var actions = new StreamAction[4];

        for (int i = 0; i < 4; i++)
        {
            var num = Random.Next(0, TotalWeight);
            MelonLogger.Msg(num);
            var action = Weights.First(x => num > x.Key.Item1 && num < x.Key.Item2).Value;
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