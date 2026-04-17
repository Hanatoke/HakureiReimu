using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class HakureiDanmakuBoundary : AbstractCounterCard {
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

        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            List<Creature> targets = CombatState.HittableEnemies.ToList();
            RunAnimation(Character.HakureiReimu.Animation.ShotA);
            await Flash(instant);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState)
                .BeforeDamage(async () =>
                {
                    List<Task> tasks = [];
                    foreach (Creature t in targets)
                    {
                        tasks.Add(FlyingVFXCmd.DanmakuCurveToTarget(Owner.Creature,t,scale:0.8f,speedScale:2f));
                    }
                    await Task.WhenAll(tasks);
                })
                .Execute(null);
            if (cost&&targets.Count>1)
            {
                await Decrement();
            }
        }
    }
}
