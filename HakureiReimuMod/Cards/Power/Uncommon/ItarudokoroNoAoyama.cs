using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Uncommon {
    public class ItarudokoroNoAoyama : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => new List<BlockVar>() {new(5,ValueProp.Move)};
        
        public ItarudokoroNoAoyama(
            ) : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }

        protected override void OnUpgrade() => this.DynamicVars.Block.UpgradeValueBy(3);
    }
}
