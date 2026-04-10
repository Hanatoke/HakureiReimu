using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Rare {
    public class UnfetteredForm : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<UnfetteredFormPower>(2)
        ];
        public UnfetteredForm(
            ) : base(3, CardType.Power, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<UnfetteredFormPower>(Owner.Creature,
                DynamicVars[UnfetteredFormPower.ID].BaseValue, Owner.Creature, this);
        }
        protected override void OnUpgrade() 
        {
            DynamicVars[UnfetteredFormPower.ID].UpgradeValueBy(1);
        }
    }
}
