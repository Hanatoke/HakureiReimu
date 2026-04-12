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
    public class YinYangKijinGyoku : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<YinYangKijinGyokuPower>(1)
        ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromOrb<YinYangOrb>()
        ];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongB;
        public YinYangKijinGyoku(
            ) : base(2, CardType.Power, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<YinYangKijinGyokuPower>(Owner.Creature,
                DynamicVars[YinYangKijinGyokuPower.ID].BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade() 
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
