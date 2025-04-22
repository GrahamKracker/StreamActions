using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Random = System.Random;

namespace StreamActions.Actions;

public class TowersFollowMouse : StreamAction
{
    /// <inheritdoc />
    public override void OnChosen()
    {
        var towers = InGame.instance.bridge.GetAllTowers().ToList();

        foreach (var tower in towers)
        {
            var towerModel = tower.tower.rootModel.Cast<TowerModel>().Duplicate();
            foreach (var attackModel in towerModel.GetBehaviors<AttackModel>())
            {
                var targetPointerModel = new TargetPointerModel("TargetPointerModel_", true, false,
                    TargetType.FollowTouch, false);
                attackModel.AddBehavior(targetPointerModel);
                attackModel.AddBehavior(new RotateToPointerModel("RotateToPointerModel_", 180, false, false, 7));

                attackModel.RemoveBehavior<TargetFirstModel>();
                attackModel.RemoveBehavior<TargetLastModel>();
                attackModel.RemoveBehavior<TargetCloseModel>();
                attackModel.RemoveBehavior<TargetStrongModel>();

                attackModel.targetProvider = targetPointerModel;
            }

            towerModel.targetTypes = new Il2CppReferenceArray<TargetType>(1)
            {
                [0] = new TargetType(TargetType.FollowTouch, false, false, false),
            };

            towerModel.UpdateTargetProviders();
            tower.tower.UpdateRootModel(towerModel);
            tower.tower.UpdateTargetType();
            tower.tower.SetTargetType(new TargetType(TargetType.FollowTouch, false, false, false));
            tower.tower.SetLockTargetTypeSwitching(true);
        }
    }

    /// <inheritdoc />
    protected override Rarity Weight => Rarity.Epic;

    /// <inheritdoc />
    public override string ChoiceText => "Placed towers now target cursor. Removed on upgrade";

    /// <inheritdoc />
    protected override bool? IsPositiveEffect => null;

    /// <inheritdoc />
    protected internal override void BeforeVoting(Random rand)
    {

    }
}