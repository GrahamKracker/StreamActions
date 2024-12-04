using System;
using Il2CppAssets.Scripts.Unity;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace StreamActions.Actions.TimedActions;

[RegisterTypeInIl2Cpp]
public class RenderReplacement(IntPtr ptr) : MonoBehaviour(ptr)
{
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(Texture2D.blackTexture, dest);
    }
}

public class GameIsInvisible : TimedAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        Game.instance.sceneCamera.gameObject.AddComponent<RenderReplacement>();
    }

    /// <inheritdoc />
    public override void OnEnd()
    {
        var replacement = Game.instance.sceneCamera.gameObject.GetComponent<RenderReplacement>();
        if (replacement != null)
        {
            Object.Destroy(replacement);
        }
    }

    /// <inheritdoc />
    protected override Rarity Weight => (Rarity)100_000;// Rarity.Rare;

    /// <inheritdoc />
    public override string ChoiceText => $"Hides the game from view for {Duration}s";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => false;
    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {
        Duration = rand.Next(10, 30);
    }
}