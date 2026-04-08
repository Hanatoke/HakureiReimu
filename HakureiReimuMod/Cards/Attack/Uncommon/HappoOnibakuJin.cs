using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class HappoOnibakuJin : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
                new DamageVar(6,ValueProp.Move),
                new CalculationBaseVar(0),
                new CalculationExtraVar(1),
                new CalculatedVar("CalculatedTimes").WithMultiplier((c,_)=>c.Owner.GetAllCounterCards().Count)
            ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(Counter)
        ];

        public HappoOnibakuJin(
            ) : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int n = (int)((CalculatedVar)DynamicVars["CalculatedTimes"]).Calculate(cardPlay.Target);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).WithHitCount(n)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(2);
        }
    }
}
