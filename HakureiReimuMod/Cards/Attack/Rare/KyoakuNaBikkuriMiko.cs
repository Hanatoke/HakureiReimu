using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class KyoakuNaBikkuriMiko : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
                new CounterVar(3),
                new DamageVar(9,ValueProp.Move),
                new CalculationBaseVar(1),
                new CalculationExtraVar(1),
                new CalculatedVar("CalculatedTimes").WithMultiplier((c,_)=>(c as KyoakuNaBikkuriMiko)?.ActivateTimes??0)
            ];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Attack,Immediate];

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

        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            if (target is not { IsHittable: true }) return;
            await Flash(instant);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target)
                .WithHitCount((int)((CalculatedVar)DynamicVars["CalculatedTimes"]).Calculate(target))
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(null);
            ActivateTimes++;
            if (cost)
            {
                await Decrement();
            }
        }
    }
}
