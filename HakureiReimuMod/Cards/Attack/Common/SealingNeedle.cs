using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.HoverTips;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class SealingNeedle : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.Seal>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new DamageVar(6, ValueProp.Move), new PowerVar<Powers.Seal>(3),new CardsVar(1)];
        
        public SealingNeedle(
            ) : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .Execute(choiceContext);
            await PowerCmd.Apply<Powers.Seal>(cardPlay.Target, DynamicVars[Powers.Seal.ID].BaseValue,
                Owner.Creature, this);
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(2);
            DynamicVars[Powers.Seal.ID].UpgradeValueBy(1);
        }
    }
}
