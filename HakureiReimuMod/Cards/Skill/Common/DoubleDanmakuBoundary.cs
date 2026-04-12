using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards.Attack.Common;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class DoubleDanmakuBoundary : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [HoverTipFactory.FromCard<DanmakuBoundary>(IsUpgraded)];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongA;
        public DoubleDanmakuBoundary(
            ) : base(0, CardType.Skill, CardRarity.Common, TargetType.None) {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            IEnumerable<CardModel> cards = await DanmakuBoundary.CreateInHand(Owner, DynamicVars.Cards.IntValue, CombatState);
            if (IsUpgraded)
            {
                foreach (CardModel c in cards)
                {
                    CardCmd.Upgrade(c);
                }
            }
        }

        protected override void OnUpgrade()
        {
            
        }
    }
}
