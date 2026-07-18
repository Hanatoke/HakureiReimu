using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class SoaringTogether : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
                new PowerVar<FlightPower>(1)
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [HoverTipFactory.FromPower<FlightPower>()];

        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

        public SoaringTogether(
            ) : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly) {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            decimal amount = DynamicVars[FlightPower.ID].BaseValue;
            Creature ally = cardPlay.Target;
            if (ally != null)
            {
                await PowerCmd.Apply<FlightPower>(choiceContext, ally, amount, Owner.Creature, this);
            }
            await PowerCmd.Apply<FlightPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
            PlayerCmd.EndTurn(Owner,false);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
