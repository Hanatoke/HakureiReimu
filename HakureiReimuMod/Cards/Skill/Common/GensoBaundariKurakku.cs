using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class GensoBaundariKurakku : AbstractCard {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Counter];

        public GensoBaundariKurakku(
            ) : base(0, CardType.Skill, CardRarity.Common, TargetType.None) {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1)
            {
                Cancelable = false
            };
            CardModel c=(await CardSelectCmd.FromSimpleGrid(choiceContext, Owner.GetAllCounterCards(), Owner, prefs)).FirstOrDefault();
            if (c is AbstractPersistCard persistCard)
            {
                await PersistCardCmd.StopPersistCard(persistCard.Slot,PileType.Hand);
            }
        }

        protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
    }
}
