using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Random = System.Random;

namespace StreamActions.Actions;

public class SendNextRounds : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        for (int i = 0; i < RoundsToSend; i++)
        {
            InGame.instance.bridge.simulation.StartRound();
        }
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Common;

    /// <inheritdoc />
    public override string ChoiceText => $"Send {RoundsToSend} Round" + (RoundsToSend == 1 ? "" : "s");

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {
        RoundsToSend = 1;
        for(int i=0; i<9; i++)
        {
            var randomNum = rand.Next(1, 101);
            if (randomNum <= 50 - (i * 10))
            {
                RoundsToSend++;
            }
            else
            {
                return;
            }
        }
    }

    private int RoundsToSend { get; set; }
}