using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Rare {
    public class KanzenHyoiToYumoFuin : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromPower<SealPower>()
        ];

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<KanzenHyoiToYumoFuinPower>(1),
            new DynamicVar("Div",KanzenHyoiToYumoFuinPower.Div)
        ];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongB;
        public KanzenHyoiToYumoFuin(
            ) : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<KanzenHyoiToYumoFuinPower>(Owner.Creature,
                DynamicVars[KanzenHyoiToYumoFuinPower.ID].BaseValue, Owner.Creature, this);
        }
        protected override void OnUpgrade() 
        {
            DynamicVars[KanzenHyoiToYumoFuinPower.ID].UpgradeValueBy(1);
        }
    }
}
