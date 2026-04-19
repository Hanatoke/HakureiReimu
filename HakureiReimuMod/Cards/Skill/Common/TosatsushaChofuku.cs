using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class TosatsushaChofuku : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [HoverTipFactory.FromPower<InhibitPower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new PowerVar<InhibitPower>(1)];

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public TosatsushaChofuku(
            ) : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<InhibitPower>(CombatState.HittableEnemies, DynamicVars[InhibitPower.ID].BaseValue,
                Owner.Creature, this);
        }
        protected override void OnUpgrade() {
            DynamicVars[InhibitPower.ID].UpgradeValueBy(1);
        }
    }
}
