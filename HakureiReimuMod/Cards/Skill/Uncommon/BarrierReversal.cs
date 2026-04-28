using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class BarrierReversal : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => 
            [
                EnergyHoverTip
            ];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DynamicVar("Num",3),
                new EnergyVar(1),
                new CalculationBaseVar(0),
                new CalculationExtraVar(1),
                new CalculatedVar("CalculatedEnergy")
                    .WithMultiplier((c,_)=>
                        Math.Floor((c.Owner.PlayerCombatState?.DiscardPile.Cards.Count??0)/c.DynamicVars["Num"].BaseValue))
            ];

        public BarrierReversal(
            ) : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int n = (int)((CalculatedVar)DynamicVars["CalculatedEnergy"]).Calculate(cardPlay.Target);
            await PlayerCmd.GainEnergy(n, Owner);
            await CardPileCmd.Shuffle(choiceContext, Owner);
        }
        protected override void OnUpgrade() {
            AddKeyword(CardKeyword.Retain);
        }
    }
}
