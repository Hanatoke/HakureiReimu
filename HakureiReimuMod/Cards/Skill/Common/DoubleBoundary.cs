using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class DoubleBoundary : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new CounterVar(2),new BlockVar(7,ValueProp.Move)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Attack,Immediate];
        public override bool GainsBlock => true;

        public DoubleBoundary(
            ) : base(2, CardType.Skill, CardRarity.Common, TargetType.Self) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }

        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            DynamicVars.Block.UpgradeValueBy(3);
        }
        public override bool IsImmediate => true;
        public override CounterType ActivateType => CounterType.Attack;
        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            await Flash(instant);
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block,null,true);
            if (cost)
            {
                await Decrement();
            }
        }
    }
}
