using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.Tracking;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Random = System.Random;

namespace StreamActions.Actions.TimedActions;

public class DoubleMoney : TimedAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Rare;

    /// <inheritdoc />
    public override string ChoiceText => $"Doubles all money earned for {Duration}s";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => true;

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {
        Duration = rand.Next(30, 50);
    }


    [HarmonyPatch(typeof(Simulation), nameof(Simulation.AddCash))]
    [HarmonyPrefix]
    private static void UnityToSimulation_AddCash(ref double c)
    {
        if (IsActive<DoubleMoney>())
        {
            c *= 2;
        }
    }
}