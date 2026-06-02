using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Uncommon {
    public class NoIntervalBoundary : AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            new HoverTip(TitleLocString,Tip),
            HoverTipFactory.FromKeyword(Counter)
        ];

        private LocString _tip;
        public LocString Tip => _tip ??= new LocString("cards", Id.Entry + ".tip");
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongA;
        public NoIntervalBoundary(
            ) : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<NoIntervalBoundaryPower>(choiceContext, Owner.Creature, DynamicVars.Cards.BaseValue,
                Owner.Creature, this);
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
