using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class DiffusiveSpiritCharm : AbstractCard
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public DiffusiveSpiritCharm(
            ) : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            CardSelectorPrefs prefs = new (this.SelectionScreenPrompt, 1)
            {
                Cancelable = false,
            };
            CardModel card =
                (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, c => c.Type == CardType.Attack, this))
                .FirstOrDefault();
            if (card != null)
            {
                foreach (CardModel c in Owner.PlayerCombatState.Hand.Cards.ToList())
                {
                    if (c!=card)
                    {
                        CardModel copy = card.CreateClone();
                        await CardCmd.Transform(c, copy);
                    }
                }
            }
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
