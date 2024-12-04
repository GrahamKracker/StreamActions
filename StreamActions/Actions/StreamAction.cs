using System;
using System.Collections.Generic;
using BTD_Mod_Helper.Api;
using UnityEngine;
using Random = System.Random;

namespace StreamActions.Actions;

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

    public static int TotalWeight { get; set; }

    protected internal static Random Random { get; } = new();

    internal static readonly Dictionary<Tuple<int, int>, StreamAction> Weights = new();

    protected internal abstract void BeforeVoting(Random rand);

    /// <inheritdoc />
    public override void Register()
    {
        Weights.Add(new Tuple<int, int>(TotalWeight, TotalWeight += (int) Weight), this);
    }

    protected enum Rarity
    {
        Common = 1000,
        Rare = 250,
        Epic = 100,
        Legendary = 50,
    }
}