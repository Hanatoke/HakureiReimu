using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class PaparatchiGekitaiKekkai : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new CounterVar(2),new CardsVar(2)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Debuff,CardKeyword.Exhaust];

        public PaparatchiGekitaiKekkai(
            ) : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }
        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            DynamicVars[CounterVar.DefaultName].UpgradeValueBy(1);
        }
        public override CounterType ActivateType => CounterType.Debuff;
        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            await Flash(instant);
            await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), DynamicVars.Cards.IntValue, Owner);
            if (cost)
            {
                await Decrement();
            }
        }
    }
}
