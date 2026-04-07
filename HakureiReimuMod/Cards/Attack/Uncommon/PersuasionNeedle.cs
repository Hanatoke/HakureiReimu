using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class PersuasionNeedle : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.SealPower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new PowerVar<Powers.SealPower>(5),
                new CalculationBaseVar(0),
                new ExtraDamageVar(1),
                new CalculatedDamageVar(ValueProp.Move).WithMultiplier((_,t)=>t?.GetPowerAmount<Powers.SealPower>() ?? 0)
            ];
        
        public PersuasionNeedle(
            ) : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<Powers.SealPower>(cardPlay.Target, DynamicVars[Powers.SealPower.ID].BaseValue,
                Owner.Creature, this);
            await DamageCmd.Attack(DynamicVars.CalculatedDamage.Calculate(cardPlay.Target)).FromCard(this).Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
