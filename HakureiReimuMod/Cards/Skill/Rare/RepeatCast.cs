using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using HakureiReimu.HakureiReimuMod.PersistCard.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class RepeatCast : AbstractCard
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public RepeatCast(
            ) : base(1, CardType.Skill, CardRarity.Rare, TargetType.None) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            CardPile hand = Owner.PlayerCombatState.Hand;
            int count = CardPile.maxCardsInHand-hand.Cards.Count;
            if (count <= 0)return;
            List<CardModel> cards = CombatManager.Instance.History.CardPlaysFinished.Where(p =>
                    p.HappenedThisTurn(CombatState)&& p.Actor==Owner.Creature && p.CardPlay.Card is not RepeatCast &&
                    p.CardPlay.Card.Type != CardType.Status && p.CardPlay.Card.Type != CardType.Curse&&!hand.Cards.Contains(p.CardPlay.Card))
                .Select(p => p.CardPlay.Card).Reverse().Distinct().ToList();
            foreach (CardModel c in cards)
            {
                if (count>0)
                {
                    count--;
                }
                else break;
                if (c.Pile is AbstractPersistCardTable table)
                {
                    await PersistCardCmd.StopPersistCard(table.GetSlot(c), PileType.Hand);
                }
                else if (c.Type==CardType.Power)
                {
                    await CardPileCmd.AddGeneratedCardToCombat(c.CreateClone(), PileType.Hand, true);
                }
                else
                {
                    await CardPileCmd.Add(c, PileType.Hand);
                }
            }
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
