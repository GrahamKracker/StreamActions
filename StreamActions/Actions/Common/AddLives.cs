using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Random = System.Random;

namespace StreamActions.Actions;

public class AddLives : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        InGame.instance.AddHealth(LifeCount);
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Common;

    /// <inheritdoc />
    public override string ChoiceText => $"Add {LifeCount} lives";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => true;

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {
        var currentRound = InGame.instance.GetSimulation().GetCurrentRound();
        LifeCount = rand.Next(5 + currentRound, 15 + currentRound);
    }

    private int LifeCount { get; set; }
}