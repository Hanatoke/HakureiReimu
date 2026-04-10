using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Rare {
    public class DoubleGreatBoundary : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<DoubleGreatBoundaryPower>(1)
        ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(Counter)
        ];
        public DoubleGreatBoundary(
            ) : base(3, CardType.Power, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<DoubleGreatBoundaryPower>(Owner.Creature,
                DynamicVars[DoubleGreatBoundaryPower.ID].BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade() 
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
