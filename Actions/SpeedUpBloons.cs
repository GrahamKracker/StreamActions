using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Track;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Random = System.Random;

namespace StreamActions.Actions;

public class SpeedUpBloons : StreamAction
{
    [HarmonyPatch(typeof(Spawner), nameof(Spawner.Emit))]
    [HarmonyPostfix]
    private static void Spawner_Emit(Spawner __instance, BloonModel bloonModel)
    {
        MelonLogger.Msg("Spawner::Emit with speed: " + bloonModel.Speed);
    }
    /// <inheritdoc />
    public override void OnChosen()
    {
        foreach (var bloon in CosmeticHelper.coopPlayerBloonMods.Values().SelectMany(x=>x.bloonsByName.Values()).Select(x=>x.bloonModel))
        {
            //bloon.Speed += (bloon.Speed * (PercentageSpeedUp/*/100f*/));
            bloon.Speed *= (1000);
            MelonLogger.Msg("Speed: " + bloon.Speed);
            MelonLogger.Msg("cosmetic speed: " + CosmeticHelper.GetBloonModel(bloon.id, 0).bloonModel.Speed);
        }
    }

    /// <inheritdoc />
    protected override Rarity Weight => (Rarity) 100000;

    /// <inheritdoc />
    public override string ChoiceText => $"Speed up Bloons by {PercentageSpeedUp}%";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;

    /// <inheritdoc />
    protected override void BeforeSelection(Random rand)
    {
        PercentageSpeedUp = rand.Next(1 + (InGame.instance.GetSimulation().GetCurrentRound() / 2), 50 + InGame.instance.GetSimulation().GetCurrentRound());
    }

    private int PercentageSpeedUp {get; set;}
}