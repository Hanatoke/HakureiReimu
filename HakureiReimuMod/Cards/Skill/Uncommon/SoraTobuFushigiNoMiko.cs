using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class SoraTobuFushigiNoMiko : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new PowerVar<DexterityPower>(4)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DexterityPower>()];

        public SoraTobuFushigiNoMiko(
            ) : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<SoraTobuFushigiNoMikoPower>(Owner.Creature, DynamicVars.Dexterity.IntValue,
                Owner.Creature, this);
        }
        protected override void OnUpgrade() {
            DynamicVars.Dexterity.UpgradeValueBy(1);
        }
    }
}
