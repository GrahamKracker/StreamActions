using System;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;

namespace StreamActions.Actions;

public class LoseRandomCashAmount : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        InGame.instance.AddCash(-CashAmount);
    }

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {
        var roundMult = (InGame.Bridge.GetCurrentRound() + 1) * .25f; //TODO find a fair algorithm
        CashAmount = rand.Next((int)(100 * roundMult), (int)(1000 * roundMult));
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Common;

    private int CashAmount { get; set; }

    /// <inheritdoc />
    public override string ChoiceText => $"Lose ${CashAmount}";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;
}