using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Track;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Random = System.Random;

namespace StreamActions.Actions;

public class PlaceRandomTowers : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        var towers = InGame.instance.GetGameModel().towers;

        for (int i = 0; i < TowersToAdd; i++)
        {
            var tower = towers[UnityEngine.Random.Range(0, towers.Count)];
            var position = new Vector3(UnityEngine.Random.Range(-116, 116), UnityEngine.Random.Range(-150, 150));
            var canPlaceReturnData = new CanPlaceReturnData();
            if (!InGame.instance.GetMap().CanPlace(position.ToVector2(), tower, null, canPlaceReturnData))
            {
                i--;
                continue;
            }

            InGame.instance.GetTowerManager().CreateTower(tower, position, InGame.instance.bridge.MyPlayerNumber, InGame.instance.GetMap().GetAreaAtPoint(position.ToVector2())?.GetAreaID() ??
                ObjectId.FromData(1), ObjectId.FromData(4294967295), null, false, false);
        }
    }

    /// <inheritdoc />
    public override Rarity Weight => Rarity.Epic;

    /// <inheritdoc />
    public override string ChoiceText => $"Place {TowersToAdd} random tower{(TowersToAdd == 1 ? "" : "s")} at a random spot";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => true;

    /// <inheritdoc />
    protected override void BeforeSelection(Random rand)
    {
        TowersToAdd = 1;
        for (int i = 0; i < 5; i++)
        {
            var randomNum = rand.Next(1, 101);
            if (randomNum <= 25 - (i * 5))
            {
                TowersToAdd++;
            }
            else
            {
                return;
            }
        }
    }

    private int TowersToAdd { get; set; }
}