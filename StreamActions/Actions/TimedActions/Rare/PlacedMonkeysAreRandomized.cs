using System;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.AbilitiesMenu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Random = System.Random;

namespace StreamActions.Actions.TimedActions;

public class PlacedMonkeysAreRandomized : TimedAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Rare;

    /// <inheritdoc />
    public override string ChoiceText =>
        $"Placed or upgraded towers are randomized with a similar amount of upgrades for {Duration}s";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => null;

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {
        Duration = rand.Next(10, 30);
    }



    [HarmonyPatch(typeof(InputManager), nameof(InputManager.TryPlace))]
    [HarmonyPrefix]
    private static void InputManager_TryPlace(InputManager __instance)
    {
        if (IsActive<PlacedMonkeysAreRandomized>())
        {
            __instance.placementModel = GetSimilar(__instance.placementModel);
        }
    }

    [HarmonyPatch(typeof(TowerManager), nameof(TowerManager.UpgradeTower))]
    [HarmonyPrefix]
    private static bool TowerManager_UpgradeTower(Il2CppAssets.Scripts.Simulation.Towers.Tower tower, TowerModel def)
    {
        if (IsActive<PlacedMonkeysAreRandomized>())
        {
            var randomTower = GetSimilar(def);
            var newtower = InGame.instance.GetTowerManager()
                .CreateTower(randomTower, tower.Position,
                    InGame.Bridge.MyPlayerNumber, tower.areaPlacedOn, tower.ParentId, null, false, false);
            var tts = newtower.GetTowerToSim();

            newtower.AddPoppedCash(tower.cashEarned);
            newtower.appliedCash = tower.GetAppliedCash();
            newtower.damageDealt = tower.damageDealt;
            newtower.worth = tower.worth;
            newtower.shouldShowCashIconInstead = tower.shouldShowCashIconInstead;

            //no more base tower from here down
            InGame.instance.GetTowerManager().DestroyTower(tower, InGame.Bridge.MyPlayerNumber);

            TowerSelectionMenu.instance.DeselectTower();
            InGame.instance.inputManager.UpdateRangeMeshes();
            InGame.instance.inputManager.SetSelected(tts);
            TowerSelectionMenu.instance.SelectTower(tts);


            AbilityMenu.instance.RebuildAbilities();
            AbilityMenu.instance.AbilitiesChanged();
            newtower.UpdateThrowCache();
            newtower.UpdateBuffs();
            newtower.UpdateThrowLocation();
            newtower.UpdateTargetType();
            newtower.UpdateRoundMutators();

            return false;
        }

        return true;
    }

    private static TowerModel GetSimilar(TowerModel model)
    {
        var maxPath = model.tiers.IndexOf(model.tiers.Max());
        var maxTier = model.tiers[maxPath];

        var similarTowers = InGame.instance.GetGameModel().towers.Where(x => IsNormalTower(x) && x.tiers[x.tiers.IndexOf(x.tiers.Max())] <= maxTier).ToList();

        var similarTower = similarTowers[Random.Next(similarTowers.Count)];

        return similarTower;
    }

    private static bool IsNormalTower(TowerModel tower) => !tower.name.Contains("Transformed", StringComparison.InvariantCultureIgnoreCase) && tower.IsStandardTower() && tower is { isSubTower: false, isGeraldoItem: false, isPowerTower: false };
}