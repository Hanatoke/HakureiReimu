using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class HakureiDanmakuBoundary : EffectFollowCard<NDanmaku> {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [new CounterVar(3),new DamageVar(7,ValueProp.Move)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [All];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellFastA;
        public HakureiDanmakuBoundary(
            ) : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) {
        }
        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(2);
        }
        public override CounterType ActivateType => CounterType.All;
        protected override NDanmaku CreateInstance =>NDanmaku.Create((float)GD.RandRange(0.9, 1.1), glow: false);
        public override void CreateEffect()
        {
            this.AddFollow(CreateInstance,(float)GD.RandRange(200f, 400f),(float)GD.RandRange(150f, 300f));
        }

        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            List<Creature> targets = CombatState.HittableEnemies.ToList();
            if (targets.Count<=1)cost=false;
            RunAnimation(Character.HakureiReimu.Animation.ShotA);
            await Flash(instant);
            if (UseEffect() is {} danmaku)
            {
                (Vector2,float,Color) set=(danmaku.GlobalPosition,danmaku.Scale.X,danmaku.ColorAble.Modulate);
                danmaku.QueueFreeSafely();
                foreach (Creature t in targets)
                {
                    FlyingVFXCmd.DanmakuCurveToTarget(set.Item1, t,1, scale:set.Item2,2f, color: set.Item3);
                }
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
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            if (cost)
            {
                await Decrement();
            }
        }
    }
}
