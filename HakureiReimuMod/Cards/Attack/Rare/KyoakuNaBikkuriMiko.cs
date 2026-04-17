using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class KyoakuNaBikkuriMiko : EffectFollowCard<NDanmaku> {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
                new CounterVar(3),
                new DamageVar(9,ValueProp.Move),
                new CalculationBaseVar(1),
                new CalculationExtraVar(1),
                new CalculatedVar("CalculatedTimes").WithMultiplier((c,_)=>(c as KyoakuNaBikkuriMiko)?.ActivateTimes??0)
            ];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Attack,Immediate];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongA;
        public KyoakuNaBikkuriMiko(
            ) : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }

        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(3);
        }
        public override bool IsImmediate => true;
        public override CounterType ActivateType => CounterType.Attack;
        public int ActivateTimes = 0;
        public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side==Owner.Creature.Side)
            {
                ActivateTimes = 0;
            }
            return Task.CompletedTask;
        }
        protected override NDanmaku CreateInstance => NDanmaku.Create((float)GD.RandRange(0.6, 0.8), glow: false);

        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            if (target is not { IsHittable: true }) return;
            RunAnimation(Character.HakureiReimu.Animation.ShotA);
            await Flash(instant);
            int count = (int)((CalculatedVar)DynamicVars["CalculatedTimes"]).Calculate(target);
            if (UseEffect() is {} danmaku)
            {
                (Vector2,float,Color) set=(danmaku.GlobalPosition,danmaku.Scale.X,danmaku.ColorAble.Modulate);
                danmaku.QueueFreeSafely();
                FlyingVFXCmd.DanmakuCurveToTarget(set.Item1, target,count, scale:set.Item2,1.5f, color: set.Item3);
                if (!instant)
                {
                    await Cmd.Wait(ActivateThisTurn switch
                    {
                        1=>0.3f,
                        2=>0.2f,
                        3=>0.1f,
                        _=>0,
                    });
                }
                if (!cost)CreateEffect();
            }
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target)
                .WithHitCount(count)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            ActivateTimes++;
            if (cost)
            {
                await Decrement();
            }
        }
    }
}
