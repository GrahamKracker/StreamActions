using System;
using System.Collections.Generic;
using BTD_Mod_Helper.Api;

namespace StreamActions;

public abstract class StreamAction : NamedModContent
{
    public abstract void OnSelected();

    protected abstract int Weight { get; }

    public abstract string ChoiceText { get; }

    private static int TotalWeight { get; set; }

    public static Random Random { get; } = new();

    private static readonly Dictionary<Tuple<int, int>, StreamAction> Weights = new();

    /// <inheritdoc />
    public override void Register()
    {
        Weights.Add(new Tuple<int, int>(TotalWeight, TotalWeight + Weight), this);
        TotalWeight += Weight;
    }

    public static StreamAction[] GetRandomActionSelections(int amount = 4)
    {
        var actions = new List<StreamAction>();

        for (int i = 0; i < amount; i++)
        {
            var num = Random.Next(0, TotalWeight);
            actions.Add(Weights.First(x => num > x.Key.Item1 && num < x.Key.Item2).Value);
        }

        return actions.ToArray();
    }
}