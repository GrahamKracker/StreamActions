using System.Collections.Immutable;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Track;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppSystem;
using Random = System.Random;

namespace StreamActions.Actions;

public class SpeedUpBloons : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        foreach (var bloon in InGame.instance.GetSimulation().model.bloons)
        {
            bloon.Speed += (bloon.Speed * (PercentageSpeedUp / 100f));
        }

        foreach(var bloon in CosmeticHelper.coopPlayerBloonMods[InGame.instance.GetUnityToSimulation().MyPlayerNumber].bloonsByName._values)
        {
            bloon.bloonModel.Speed += (bloon.bloonModel.Speed * (PercentageSpeedUp / 100f));
        }
    }

    /// <inheritdoc />
    public override Rarity Weight => Rarity.Rare;

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