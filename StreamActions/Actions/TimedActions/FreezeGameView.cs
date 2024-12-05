using System;
using Il2CppAssets.Scripts.Unity;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace StreamActions.Actions.TimedActions;

public class FreezeGameView : TimedAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        Game.instance.sceneCamera.gameObject.SetActive(false);
    }

    /// <inheritdoc />
    public override void OnEnd()
    {
        Game.instance.sceneCamera.gameObject.SetActive(true);
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Rare;

    /// <inheritdoc />
    public override string ChoiceText => $"Freezes the game view for {Duration}s";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;
    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {
        Duration = rand.Next(10, 30);
    }
}