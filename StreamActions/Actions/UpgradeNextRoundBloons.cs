using System.Collections.Generic;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.BloonMenu;
using Random = System.Random;

namespace StreamActions.Actions;

public class UpgradeNextRoundBloons : StreamAction
{
    private static readonly Dictionary<string, string> NextBloonTiers = new()
    {
        { BloonType.Red, BloonType.Blue },
        { BloonType.Blue, BloonType.Green },
        { BloonType.Green, BloonType.Yellow },
        { BloonType.Yellow, BloonType.Pink },
        { BloonType.Pink, BloonType.Black},
        { BloonType.Black, BloonType.Zebra },
        { BloonType.White, BloonType.Zebra },
        { BloonType.Purple, BloonType.Rainbow },
        { BloonType.Zebra, BloonType.Rainbow },
        { BloonType.Lead, BloonType.Rainbow },
        { BloonType.Rainbow, BloonType.Ceramic},
        { BloonType.Ceramic, BloonType.Moab },
        { BloonType.Moab, BloonType.Bfb },
        { BloonType.Bfb, BloonType.Zomg },
        { BloonType.Zomg, BloonType.Ddt },
        { BloonType.Ddt, BloonType.Bad },
        { BloonType.Bad, BloonType.Bad },
    };

    /// <inheritdoc />
    public override void OnChosen()
    {
        var round = InGame.instance.GetGameModel().roundSet.rounds[InGame.instance.bridge.GetCurrentRound() + 1];
        if (round != null)
        {
            foreach (var bloonGroupModel in round.groups)
            {
                var bloonModel = bloonGroupModel.GetBloonModel();
                if (NextBloonTiers.TryGetValue(bloonModel.baseId, out var newBloon))
                {
                    InGame.instance.GetGameModel().GetBloon(newBloon).FindChangedBloonId(b =>
                    {
                        b.SetCamo(bloonModel.IsCamoBloon());
                        b.SetFortified(bloonModel.IsFortifiedBloon());
                        if (bloonModel.IsRegrowBloon())
                            b.SetRegrow(bloonModel.GetBehavior<GrowModel>().growToId,
                                bloonModel.GetBehavior<GrowModel>().rate);
                    }, out var newId);
                    bloonGroupModel.bloon = newId;
                }
            }

            round.emissions_ = null;
        }
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Common;

    /// <inheritdoc />
    public override string ChoiceText => "Upgrades each bloon in the next round by 1 tier";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {
    }
}