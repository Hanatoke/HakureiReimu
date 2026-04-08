using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Uncommon {
    public class JinjaHanjoKigan : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<ProtectionFeePower>(12)
        ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.Static(StaticHoverTip.Fatal),
        ];

        public override bool CanBeGeneratedInCombat => false;

        public JinjaHanjoKigan(
            ) : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<ProtectionFeePower>(Owner.Creature,
                DynamicVars[ProtectionFeePower.ID].BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade() 
        {
            DynamicVars[ProtectionFeePower.ID].UpgradeValueBy(4);
        }
    }
}
