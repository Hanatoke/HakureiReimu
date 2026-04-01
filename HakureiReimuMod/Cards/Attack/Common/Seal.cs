using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.HoverTips;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class Seal : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.Seal>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new DamageVar(3, ValueProp.Move), new PowerVar<Powers.Seal>(3)];
        
        public Seal(
            ) : base(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .Execute(choiceContext);
            await PowerCmd.Apply<Powers.Seal>(cardPlay.Target, DynamicVars[Powers.Seal.ID].BaseValue,
                Owner.Creature, this);
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(1);
            DynamicVars[Powers.Seal.ID].UpgradeValueBy(1);
        }
    }
}
