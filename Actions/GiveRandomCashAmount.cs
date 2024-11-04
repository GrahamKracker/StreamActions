using System;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;

namespace StreamActions.Actions;

public class GiveRandomCashAmount : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        InGame.instance.AddCash(CashAmount);
    }

    /// <inheritdoc />
    protected override void BeforeSelection(Random rand)
    {
        var roundMult = (InGame.Bridge.GetCurrentRound() + 1) * .25f; //TODO find a fair algorithm
        CashAmount = rand.Next((int)(100 * roundMult), (int)(1000 * roundMult));
    }

    /// <inheritdoc />
    public override Rarity Weight => Rarity.Common;

    private int CashAmount { get; set; }

    /// <inheritdoc />
    public override string ChoiceText => $"Give ${CashAmount}";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => true;
}