using Random = System.Random;

namespace StreamActions.Actions.TimedActions;

public class PlacedMonkeysAreRandomized : TimedAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {

    }

    /// <inheritdoc />
    public override Rarity Weight => (Rarity)100_000;// Rarity.Rare;

    /// <inheritdoc />
    public override string ChoiceText => "Placed or upgraded towers are randomized with a similar amount of upgrades";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect { get; }

    /// <inheritdoc />
    protected override void BeforeSelection(Random rand)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public override int Duration => 20;
}