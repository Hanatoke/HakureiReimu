using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class SubspaceAcupoint : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.Seal>(), HoverTipFactory.FromPower<WeakPower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new PowerVar<Powers.Seal>(6),new PowerVar<WeakPower>(1)];
        
        public SubspaceAcupoint(
            ) : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<Powers.Seal>(cardPlay.Target, DynamicVars[Powers.Seal.ID].BaseValue,
                Owner.Creature, this);
            await PowerCmd.Apply<WeakPower>(cardPlay.Target,DynamicVars.Weak.BaseValue,
                Owner.Creature, this);
        }
        protected override void OnUpgrade() {
            DynamicVars[Powers.Seal.ID].UpgradeValueBy(1);
            DynamicVars.Weak.UpgradeValueBy(1);
        }
    }
}
