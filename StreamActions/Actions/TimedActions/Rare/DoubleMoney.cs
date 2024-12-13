using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Random = System.Random;

namespace StreamActions.Actions.TimedActions;

public class DoubleMoney : TimedAction
{
    private static bool Active; //todo: make api for active bool in static context

    /// <inheritdoc />
    public override void OnChosen()
    {
        Active = true;
    }

    public override void OnEnd()
    {
        Active = false;
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
    [HarmonyPostfix]
    private static void Simulation_AddCash(ref int cash)
    {
        if (Active)
        {
            cash *= 2;
        }
    }
}