using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class HelpFromFriends : AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new EnergyVar(2),
            new DynamicVar("Retain",1)
        ];

        public override IEnumerable<CardKeyword> CanonicalKeywords => 
            [
                CardKeyword.Exhaust,
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.Static(StaticHoverTip.Energy),
            HoverTipFactory.FromKeyword(CardKeyword.Retain),
            HoverTipFactory.FromPower<RetainEnergyPower>()
        ];

        public HelpFromFriends(
            ) : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
            await PowerCmd.Apply<RetainHandPower>(Owner.Creature, DynamicVars["Retain"].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<RetainEnergyPower>(Owner.Creature, DynamicVars["Retain"].BaseValue, Owner.Creature, this);
        }
        protected override void OnUpgrade() {
            DynamicVars.Energy.UpgradeValueBy(1);
        }
    }
}
