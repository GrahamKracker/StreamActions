using System.Collections.Generic;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Random = System.Random;

namespace StreamActions.Actions.TimedActions;

public class CantSelectTowers : TimedAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        _towersToRenable.Clear();

        foreach (var tower in InGame.instance.GetTowers().Where(tower => tower.IsSelectable))
        {
            _towersToRenable.Add(tower);
            tower.SetSelectionBlocked(true);
        }
    }

    private readonly HashSet<Tower> _towersToRenable = [];

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Epic;

    /// <inheritdoc />
    public override string ChoiceText => $"Existing towers can't be selected for {Duration} seconds";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {
        Duration = rand.Next(20, 40);
    }

    /// <inheritdoc />
    public override void OnEnd()
    {
        foreach (var tower in _towersToRenable)
        {
            tower.SetSelectionBlocked(false);
        }
        _towersToRenable.Clear();
    }
}