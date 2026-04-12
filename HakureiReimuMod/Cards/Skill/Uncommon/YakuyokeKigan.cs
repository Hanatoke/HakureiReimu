using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class YakuyokeKigan : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        ];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongB;
        public YakuyokeKigan(
            ) : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1)
            {
                Cancelable = false
            };
            CardModel c=(await CardSelectCmd.FromHand(choiceContext, Owner, prefs,null,this)).FirstOrDefault();
            if (c!=null)
            {
                await CardCmd.Exhaust(choiceContext, c);
                if ((c.Type==CardType.Status||c.Type==CardType.Curse)&&c.DeckVersion is {IsRemovable:true } deck)
                {
                    await CardPileCmd.RemoveFromDeck(deck);
                }
            }
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
