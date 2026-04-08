using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class DreamSubspaceHole : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new CardsVar(3)];

        public DreamSubspaceHole(
            ) : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<ExtraDrawPower>(Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
        }
        protected override void OnUpgrade() {
            DynamicVars.Cards.UpgradeValueBy(1);
        }
    }
}
