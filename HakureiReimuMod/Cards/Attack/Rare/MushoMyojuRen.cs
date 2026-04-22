using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class MushoMyojuRen : AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DamageVar(3, ValueProp.Move),
            new CalculationBaseVar(0),
            new CalculationExtraVar(1),
            new CalculatedVar("CalculatedTimes").WithMultiplier((c, t) =>
                CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>().Count(e =>
                    e.Receiver == t && e.HappenedThisTurn(c.CombatState) && e.Dealer != null &&
                    e.Dealer.Side == c.Owner.Creature.Side))
        ];

        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongA;
        public MushoMyojuRen(
            ) : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int n = (int)((CalculatedVar)DynamicVars["CalculatedTimes"]).Calculate(cardPlay.Target);
            await StartVfx(n, Owner.Creature, cardPlay.Target);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .WithHitCount(n).Execute(choiceContext);
        }

        protected Task StartVfx(int count, Creature source, Creature target)
        {
            if (count <= 0||source==null||target==null)return Task.CompletedTask;
            TaskCompletionSource ctx = new ();
            _ = Vfx(count, source, target,ctx);
            return ctx.Task;
        }

        protected async Task Vfx(int count, Creature source, Creature target,TaskCompletionSource ctx)
        {
            NCreature s = NCombatRoom.Instance?.GetCreatureNode(source);
            NCreature t = NCombatRoom.Instance?.GetCreatureNode(target);
            bool isFirst = true;
            if (s != null && t != null)
            {
                Vector2 sourcePos = s.VfxSpawnPosition;
                Vector2 targetPos = t.VfxSpawnPosition;
                for (var i = 0; i < count; i++)
                {
                    if (NCombatRoom.Instance==null)break;
                    if (target.IsDead)break;
                    Color c = Color.FromHsv(GD.Randf(), 1, 1);
                    Vector2 v = Vector2.Right.Rotated(Mathf.DegToRad(i * -30));
                    FlyingVFX vfx = FlyingVFX.Create(new SteeringMover(sourcePos + v * 100f,
                        targetPos + FlyingVFXCmd.RandomOffset(), v * 500f, 0, 2000));
                    vfx.Duration = 3;
                    vfx.UpdateMethod = (time, _) =>
                    {
                        if (time > 0.1f)
                        {
                            SteeringMover mover = (SteeringMover)vfx.Mover;
                            mover.TurnSpeed = 10;
                        }
                    };
                    vfx.OnHit = () =>
                    {
                        NDebugAudioManager.Instance?.Play("blunt_attack.mp3");
                        FlyingVFXCmd.AddVFXOnTarget(NDanmakuImpact.Create(color:c),vfx.GlobalPosition);
                        if (isFirst)
                        {
                            isFirst = false;
                            ctx.TrySetResult();
                        }
                    };
                    vfx.AddChildSafely(NDanmaku.Create(color:c,trailLength:50));
                    NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    await Cmd.Wait(0.1f);
                }
            }
            await Cmd.Wait(1);
            ctx.TrySetResult();
        }

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(1);
        }
    }
}
