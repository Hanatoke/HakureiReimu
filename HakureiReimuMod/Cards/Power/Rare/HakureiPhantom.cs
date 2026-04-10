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
    public class HakureiPhantom : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            
        ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            
        ];
        public HakureiPhantom(
            ) : base(3, CardType.Power, CardRarity.Ancient, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            
        }

        protected override void OnUpgrade() 
        {
            
        }
    }
}
