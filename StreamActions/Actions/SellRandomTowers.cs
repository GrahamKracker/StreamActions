using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Random = System.Random;

namespace StreamActions.Actions;

public class SellRandomTowers : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        var towers = InGame.instance.bridge.GetAllTowers().ToList();

        for (int i = 0; i < TowersToSell; i++)
        {
            if(towers.Count == 0)
                return;
            var tower = towers[UnityEngine.Random.Range(0, towers.Count)];
            tower.tower.SellTower();
            towers.Remove(tower);
        }
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Common;

    /// <inheritdoc />
    public override string ChoiceText => $"Sell {TowersToSell} random tower" + (TowersToSell == 1 ? "" : "s");

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;

    /// <inheritdoc />
    protected override void BeforeVoting(Random rand)
    {
        TowersToSell = 1;
        for (int i = 0; i < 5; i++)
        {
            var randomNum = rand.Next(1, 101);
            if (randomNum <= 50 - (i * 10))
            {
                TowersToSell++;
            }
            else
            {
                return;
            }
        }
    }

    private int TowersToSell { get; set; }
}