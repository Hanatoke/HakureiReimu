using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class NoIntervalBoundary : AbstractCard
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(Counter)
        ];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongA;
        public NoIntervalBoundary(
            ) : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            foreach (CardModel c in Owner.GetAllCounterCards())
            {
                List<CardModel> cards = [];
                for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
                {
                    cards.Add(c.CreateClone());
                }
                CardCmd.PreviewCardPileAdd(await  CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Draw, true,CardPilePosition.Random));
            }
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
