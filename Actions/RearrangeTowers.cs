using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppSystem.Linq;
using Random = System.Random;

namespace StreamActions.Actions;

public class RearrangeTowers : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        var allTowers = InGame.instance.bridge.GetAllTowers().ToArray();
        foreach (var tower in allTowers.Select(x=>x.tower))
        {
            var position = new Vector3(UnityEngine.Random.Range(-116, 116), UnityEngine.Random.Range(-150, 150));
            tower.PositionTower(position.ToVector2());
        }
    }

    /// <inheritdoc />
    public override Rarity Weight => Rarity.Legendary;

    /// <inheritdoc />
    public override string ChoiceText => "Move each tower to a random position";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;

    /// <inheritdoc />
    protected override void BeforeSelection(Random rand)
    {

    }
}