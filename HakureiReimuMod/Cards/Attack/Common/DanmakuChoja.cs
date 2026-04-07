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
    public class DanmakuChoja : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3,ValueProp.Move),new RepeatVar(1)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>()];

        public DanmakuChoja(
            ) : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await YinYangOrbCmd.Spawn(choiceContext,Owner,DynamicVars.Repeat.IntValue, this);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(this.CreateClone(), PileType.Discard, true), 2f);
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(3);
        }
    }
}
