using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class DreamSealingScatter : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(14,ValueProp.Move),new PowerVar<VulnerablePower>(2)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.ShotC;
        public DreamSealingScatter(
            ) : base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await Vfx(Owner.Creature, CombatState.HittableEnemies);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this).TargetingAllOpponents(CombatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            foreach (Creature e in CombatState.HittableEnemies)
            {
                if (e.HasPower<Powers.SealPower>())
                {
                    await PowerCmd.Apply<VulnerablePower>(e, DynamicVars.Vulnerable.IntValue, Owner.Creature, this);
                }
            }
        }

        public static async Task Vfx(Creature source,IEnumerable<Creature> targets)
        {
            if (CombatManager.Instance.IsOverOrEnding)return;
            NCreature s = NCombatRoom.Instance?.GetCreatureNode(source);
            if (s==null)return ;
            List<Task> tasks = [];
            foreach (Creature target in targets)
            {
                NCreature t=NCombatRoom.Instance?.GetCreatureNode(target);
                if (t == null)continue;
                int num = GD.RandRange(3,6);
                for (var i = 0; i < num; i++)
                {
                    Color color = Color.FromHsv(GD.Randf(), 1, 1);
                    float scale = (float)GD.RandRange(0.8, 1.2f);
                    Vector2 v = Vector2.Right.Rotated(Mathf.DegToRad((float)GD.RandRange(0, 360d))) * 1000f;
                    FlyingVFX vfx=FlyingVFX.Create(new SteeringMover(s.VfxSpawnPosition,
                        t.VfxSpawnPosition + FlyingVFXCmd.RandomOffset(), v, 1, 0));
                    vfx.Duration = 3;
                    vfx.UpdateMethod = (time, _) =>
                    {
                        if (time>0.2f)
                        {
                            SteeringMover mover = (SteeringMover)vfx.Mover;
                            mover.TurnSpeed = 90;
                            mover.Acceleration = 3000;
                            mover.MaxSpeed = mover.Speed + mover.Acceleration * 2;
                        }
                    };
                    vfx.OnHit = () =>
                    {
                        NDebugAudioManager.Instance?.Play("blunt_attack.mp3");
                        FlyingVFXCmd.AddVFXOnTarget(NDanmakuImpact.Create(scale, color), vfx.GlobalPosition);
                    };
                    vfx.AddChildSafely(NDanmaku.Create(scale,color));
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
                    tasks.Add(vfx.HitTask);
                }
            }

            if (tasks.Count>0)
            {
                await Task.WhenAny(tasks);
            }
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(4);
            DynamicVars.Vulnerable.UpgradeValueBy(1);
        }
    }
}
