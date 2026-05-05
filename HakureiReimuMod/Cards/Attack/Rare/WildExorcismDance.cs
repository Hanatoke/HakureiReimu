using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class WildExorcismDance : AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DamageVar(7, ValueProp.Move),
                new PowerVar<SealPower>(3)
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SealPower>()];

        public override IEnumerable<CardKeyword> CanonicalKeywords => [IgnoreDefense,CardKeyword.Exhaust];
        protected override bool HasEnergyCostX => true;
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongA;
        public WildExorcismDance(
            ) : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int count = ResolveEnergyXValue();
            decimal seal = DynamicVars[SealPower.ID].BaseValue;
            await Vfx(Owner.Creature, CombatState.HittableEnemies, count);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState).WithHitCount(count)
                .BeforeDamage(async delegate
                {
                    await PowerCmd.Apply<SealPower>(CombatState.HittableEnemies, seal, Owner.Creature, this);
                })
            .Execute(choiceContext);
        }

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(2);
            DynamicVars[SealPower.ID].UpgradeValueBy(2);
        }

        public static async Task Vfx(Creature source, IEnumerable<Creature> targets, int count)
        {
            if (CombatManager.Instance.IsOverOrEnding)return;
            if (count <= 0) return;
            NCreature s = NCombatRoom.Instance?.GetCreatureNode(source);
            if (s==null)return ;
            List<Creature> enumerable = targets.ToList();
            _ = LaunchLoop();
            await VfxLaunch(s, enumerable);
            return;

            async Task LaunchLoop()
            {
                for (var i = 1; i < count; i++)
                {
                    await Cmd.Wait(0.25f);
                    if (CombatManager.Instance.IsOverOrEnding)return;
                    _ = VfxLaunch(s, enumerable);
                }
            }
        }

        public static async Task VfxLaunch(NCreature source, IEnumerable<Creature> targets)
        {
            List<Task> tasks = [];
            foreach (Creature target in targets)
            {
                NCreature t=NCombatRoom.Instance?.GetCreatureNode(target);
                if (t == null)continue;
                int num = GD.RandRange(1,3);
                for (var i = 0; i < num; i++)
                {
                    float scale = (float)GD.RandRange(0.8, 1.2f);
                    Vector2 v = Vector2.Right.Rotated(Mathf.DegToRad((float)GD.RandRange(0, 360d))) * 800f;
                    FlyingVFX vfx=FlyingVFX.Create(new SteeringMover(source.VfxSpawnPosition,
                        t.VfxSpawnPosition + FlyingVFXCmd.RandomOffset(), v, 2, 0));
                    vfx.Duration = 3;
                    vfx.UpdateMethod = (time, _) =>
                    {
                        if (time>0.2f)
                        {
                            SteeringMover mover = (SteeringMover)vfx.Mover;
                            mover.TurnSpeed = 2 + 8 * time;
                            mover.Acceleration = 1000 + 1000 * time;
                            mover.MaxSpeed = mover.Speed + mover.Acceleration * 2;
                        }
                    };
                    vfx.OnHit = () =>
                    {
                        NDebugAudioManager.Instance?.Play("slash_attack.mp3");
                        VfxCmd.PlayVfx(vfx.GlobalPosition,VfxCmd.slashPath);
                    };
                    vfx.AddChildSafely(NTalisman.Create(scale));
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
                    tasks.Add(vfx.HitTask);
                }
            }
            if (tasks.Count>0)
            {
                await Task.WhenAny(tasks);
            }
        }
    }
}
