using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Random = System.Random;

namespace StreamActions.Actions;

public class TowersShootSlower : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        foreach (var tower in InGame.instance.GetTowers())
        {
            tower.AddMutator(new RateSupportModel.RateSupportMutator(true, "TowersShootSlower", 0, 1, null));
        }
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Rare;

    /// <inheritdoc />
    public override string ChoiceText => "All placed towers shoot slower";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {

    }

    [HarmonyPatch(typeof(RateSupportModel.RateSupportMutator), nameof(RateSupportModel.RateSupportMutator.Mutate))]
    [HarmonyPrefix]
    private static bool Mutate(RateSupportModel.RateSupportMutator __instance, Model baseModel, Model model,
        ref bool __result)
    {
        if (__instance.id != "TowersShootSlower")
        {
            return true;
        }

        if (model.Is<TowerModel>(out var castObject))
        {
            castObject.GetBehaviors<AttackModel>().ForEach(x=>x.weapons.ForEach(y=>y.rate *= 1.2f));
            __result = true;
            return false;
        }

        return true;
    }
}
