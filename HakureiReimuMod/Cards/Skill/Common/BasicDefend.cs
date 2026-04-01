using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class BasicDefend : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5,ValueProp.Move)];
        protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
        public BasicDefend(
            ) : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self) {
        }
        public override bool GainsBlock => true;

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }

        protected override void OnUpgrade() => this.DynamicVars.Block.UpgradeValueBy(3);
    }
}
