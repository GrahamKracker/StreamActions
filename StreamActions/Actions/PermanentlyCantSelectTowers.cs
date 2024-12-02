using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Random = System.Random;

namespace StreamActions.Actions;

public class PermanentlyCantSelectTowers : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        foreach (var tower in InGame.instance.GetTowers().Where(_ => Random.Next(0, 100) < 15))
        {
            tower.SetSelectionBlocked(true);
        }
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Legendary;

    /// <inheritdoc />
    public override string ChoiceText => "Each tower has a 15% chance to be permanently unselectable";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;

    /// <inheritdoc />
    protected override void BeforeVoting(Random rand)
    {

    }
}