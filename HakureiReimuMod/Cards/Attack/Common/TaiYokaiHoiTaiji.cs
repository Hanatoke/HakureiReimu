using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class TaiYokaiHoiTaiji : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [new CalculationBaseVar(11),new ExtraDamageVar(3),
                new CalculatedDamageVar(ValueProp.Move).WithMultiplier((c,t)=>t!=null?t.Powers.Count(p => p.Type==PowerType.Buff):0)];

        public TaiYokaiHoiTaiji(
            ) : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        protected override void OnUpgrade() {
            DynamicVars.CalculationBase.UpgradeValueBy(3);
            DynamicVars.ExtraDamage.UpgradeValueBy(1);
        }
    }
}
