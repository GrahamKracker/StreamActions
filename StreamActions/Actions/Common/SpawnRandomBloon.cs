using System.Collections.Generic;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Random = System.Random;

namespace StreamActions.Actions;

public class SpawnRandomBloon : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        var currentRound = InGame.instance.GetUnityToSimulation().GetCurrentRound();
        InGame.instance.GetUnityToSimulation().SpawnBloons(Game.instance.model.CreateBloonEmissions(GetBloonForRound(
                currentRound).id, 1, 0),
            currentRound, 0);
    }

    private BloonModel GetBloonForRound(int currentRound)
    {
        var bloons = new List<BloonModel>();
        foreach (var bloonModel in InGame.instance.GetGameModel().bloons.Where(x=> x is { dontShowInSandboxOnRelease:false, dontShowInSandbox:false, isBoss: false, IsRock: false, isInvulnerable: false }))
        {
            switch (currentRound)
            {
                case < 10:
                    if (bloonModel.danger < 6)
                    {
                        bloons.Add(bloonModel);
                    }

                    break;
                case < 20:
                    if (bloonModel.danger < 7)
                    {
                        bloons.Add(bloonModel);
                    }

                    break;
                case < 30:
                    if (bloonModel.danger < 10)
                    {
                        bloons.Add(bloonModel);
                    }

                    break;
                case < 40:

                    if (bloonModel.danger < 11)
                    {
                        bloons.Add(bloonModel);
                    }

                    break;
                case < 50:
                    if (bloonModel.danger < 12)
                    {
                        bloons.Add(bloonModel);
                    }

                    break;
                case < 60:
                    if (bloonModel.danger < 13)
                    {
                        bloons.Add(bloonModel);
                    }

                    break;
                case < 70:
                    if (bloonModel.danger < 14)
                    {
                        bloons.Add(bloonModel);
                    }

                    break;
                case < 80:
                    if (bloonModel.danger <= 15)
                    {
                        bloons.Add(bloonModel);
                    }
                    break;
                case >= 80:
                    bloons.Add(bloonModel);
                    break;
            }
        }

        return bloons[Random.Next(0, bloons.Count)];
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Common;

    /// <inheritdoc />
    public override string ChoiceText => "Spawn a random bloon on the map, depending on the current round";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {

    }
}