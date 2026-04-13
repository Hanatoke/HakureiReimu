using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class Sneak : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1),new CounterVar(2)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Attack];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongA;

        public Sneak(
            ) : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)=>Task.CompletedTask;

        protected override void OnUpgrade() => DynamicVars[CounterVar.DefaultName].UpgradeValueBy(1);
        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;
        public override CounterType ActivateType => CounterType.All;

        public override async Task InvokeCounter(Creature target, CounterType byType)
        {
            if (!IsInCombat)
            {
                MainFile.Logger.Warn("尝试发动不在战斗中的反制卡? "+this.GetType().Name);
                return;
            }
            if (CounterManager.InInvokeCounter)return;
            if (CounterManager.InMonsterMove)
            {
                if (byType==CounterType.Attack)
                {
                    CounterManager.AddToLater(this, () => Task.CompletedTask);
                    CounterManager.CancelLater(this);
                }
                else
                {
                    CounterManager.AddToLater(this,async ()=>await CounterCmd.InvokeCounter(CombatState,this,target));
                }
            }
            else if (byType!=CounterType.Attack)
            {
                await CounterCmd.InvokeCounter(CombatState, this, target);
            }
        }

        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            await Flash(instant);
            if (CombatState is {CurrentSide:CombatSide.Player})
            {
                await PlayerCmd.GainEnergy(1, this.Owner);
            }
            else
            {
                await PowerCmd.Apply<EnergyNextTurnPower>(Owner.Creature, 1, Owner.Creature, this);
            }
            if (cost)
            {
                await Decrement();
            }
        }
    }
}
