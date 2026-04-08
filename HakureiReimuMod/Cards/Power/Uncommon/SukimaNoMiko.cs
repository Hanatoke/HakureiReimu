using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Uncommon {
    public class SukimaNoMiko : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<ParryPower>(3)
        ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(Counter),
            HoverTipFactory.Static(StaticHoverTip.Block)
        ];
        public SukimaNoMiko(
            ) : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<ParryPower>(Owner.Creature,
                DynamicVars[ParryPower.ID].BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade() 
        {
            DynamicVars[ParryPower.ID].UpgradeValueBy(1);
        }
    }
}
