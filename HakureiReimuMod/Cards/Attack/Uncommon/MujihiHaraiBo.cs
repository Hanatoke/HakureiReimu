using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class MujihiHaraiBo : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.SealPower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DamageVar(10,ValueProp.Move)
            ];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.AttackCloseHeavy;
        public MujihiHaraiBo(
            ) : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            AttackCommand command=await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .Execute(choiceContext);
            foreach (DamageResult result in command.Results)
            {
                if (result.Receiver is{IsHittable:true}&&result.UnblockedDamage>0)
                {
                    await PowerCmd.Apply<Powers.SealPower>(result.Receiver, result.UnblockedDamage,Owner.Creature,this);
                }
            }
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(4);
        }
    }
}
