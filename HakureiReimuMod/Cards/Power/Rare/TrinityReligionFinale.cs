using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Rare {
    public class TrinityReligionFinale : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<StrengthPower>(2),
            new PowerVar<InhibitPower>(1),
        ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromPower<StrengthPower>(),
            HoverTipFactory.FromPower<DexterityPower>(),
            HoverTipFactory.FromPower<InhibitPower>(),
        ];
        public TrinityReligionFinale(
            ) : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<StrengthPower>(Owner.Creature,
                DynamicVars.Strength.BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<DexterityPower>(Owner.Creature,
                DynamicVars.Strength.BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<InhibitPower>(Owner.Creature, DynamicVars[InhibitPower.ID].BaseValue, Owner.Creature,
                this);
        }

        protected override void OnUpgrade() 
        {
            DynamicVars.Strength.UpgradeValueBy(1);
        }
    }
}
