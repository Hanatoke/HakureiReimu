using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.HoverTips;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class DarkenedPearl : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7,ValueProp.Move),new RepeatVar(2)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>()];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.ShotA;
        public DarkenedPearl(
            ) : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .BeforeDamage(async ()=>await FlyingVFXCmd.DanmakuLineToTarget(Owner.Creature,cardPlay.Target))
                .Execute(choiceContext);
            await YinYangOrbCmd.Spawn(choiceContext,Owner,DynamicVars.Repeat.IntValue, this);
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(2);
            DynamicVars.Repeat.UpgradeValueBy(1);
        }
    }
}
