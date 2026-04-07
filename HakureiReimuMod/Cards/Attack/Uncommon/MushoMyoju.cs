using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class MushoMyoju : AbstractCard,IYinYangOrbListener {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5,ValueProp.Move),new CardsVar(1)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>()];

        public MushoMyoju(
            ) : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .BeforeDamage(async ()=>await FlyingVFXCmd.DanmakuLineToTarget(Owner.Creature,cardPlay.Target))
                .Execute(choiceContext);
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(3);
        }

        public decimal ModifyEvokeVal(YinYangOrb orb, decimal result) => result;

        public async Task AfterEvokeOrb(PlayerChoiceContext choiceContext, YinYangOrb orb, Player player, Creature target,
            CardModel cardSource)
        {
            if (cardSource==this)
            {
                await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,player);
            }
        }
    }
}
