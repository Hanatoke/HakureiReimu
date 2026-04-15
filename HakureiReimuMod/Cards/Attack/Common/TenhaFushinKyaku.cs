using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class TenhaFushinKyaku : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4,ValueProp.Move),new RepeatVar(4)];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.AttackDashHeavy;
        public TenhaFushinKyaku(
            ) : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars.Repeat.IntValue)
                .FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        protected override void OnUpgrade() {
            DynamicVars.Repeat.UpgradeValueBy(1);
        }
    }
}
