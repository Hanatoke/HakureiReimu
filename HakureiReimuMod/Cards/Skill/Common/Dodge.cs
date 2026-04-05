using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class Dodge : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7,ValueProp.Move)];
        public Dodge(
            ) : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) {
        }
        public override bool GainsBlock => true;

        protected override bool IsPlayable =>
            !CombatManager.Instance.History.CardPlaysFinished.Any(e =>
                e.HappenedThisTurn(CombatState) && e.CardPlay.Card.Type == CardType.Attack&&e.CardPlay.Card.Owner==Owner);

        protected override bool ShouldGlowGoldInternal => IsPlayable;

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }

        protected override void OnUpgrade() => this.DynamicVars.Block.UpgradeValueBy(3);
    }
}
