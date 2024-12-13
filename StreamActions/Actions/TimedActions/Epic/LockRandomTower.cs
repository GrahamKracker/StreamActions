using System;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.RightMenu;
using Random = System.Random;

namespace StreamActions.Actions.TimedActions;

public class LockRandomTower : TimedAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        if (TowerToLock != null && InGame.instance.GetTowerInventory().towerMaxes.ContainsKey(TowerToLock))
        {
            TowerToUnlock = TowerToLock;
            InGame.instance.GetTowerInventory().towerMaxes[TowerToLock] = 0;
            ShopMenu.instance.ForceRefreshTowerSet();
            ShopMenu.instance.RebuildTowerSet();
        }
    }

    /// <inheritdoc />
    public override void OnEnd()
    {
        if (!string.IsNullOrEmpty(TowerToUnlock) && InGame.instance.GetTowerInventory().towerMaxes.ContainsKey(
                TowerToUnlock))
        {
            InGame.instance.GetTowerInventory().towerMaxes[TowerToUnlock] = int.MaxValue;
            ShopMenu.instance.ForceRefreshTowerSet();
            ShopMenu.instance.RebuildTowerSet();
        }
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Epic;

    /// <inheritdoc />
    public override string ChoiceText => $"Lock {TowerToLock} from being placed for {Duration}s";

    private string? TowerToLock { get; set; }
    private string? TowerToUnlock { get; set; }

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {
        var validTowers = InGame.instance.GetGameModel().towers.Where(tower=>
            !tower.name.Contains("Transformed", StringComparison.InvariantCultureIgnoreCase) &&
            tower.IsStandardTower() && tower is { isSubTower: false, isGeraldoItem: false, isPowerTower: false, IsBaseTower: true }).ToList();
        TowerToLock = validTowers[rand.Next(0, validTowers.Count)].baseId;

        Duration = rand.Next(20, 40);
    }
}