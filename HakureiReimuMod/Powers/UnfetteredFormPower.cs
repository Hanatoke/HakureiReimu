using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class UnfetteredFormPower : AbstractPower
    {
        public static readonly string ID = nameof(UnfetteredFormPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public override decimal ModifyHandDraw(Player player, decimal count)
        {
            return player.Creature != Owner ? count : count + Amount;
        }

        public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card.Owner.Creature!=Owner)return Task.CompletedTask;
            Player player = card.Owner;
            if (player.PlayerCombatState.Hand.Cards.Count >= CardPile.maxCardsInHand ||
                player.PlayerCombatState.DrawPile.Cards.Count <= 0 &&
                player.PlayerCombatState.DiscardPile.Cards.Count <= 0) 
            {
                Trigger(player);
            }
            return Task.CompletedTask;
        }

        public void Trigger(Player player)
        {
            Flash();
            IReadOnlyList<CardModel> cards = PileType.Hand.GetPile(player).Cards;
            Rng combatCardSelection = player.RunState.Rng.CombatCardSelection;
            CardModel cardModel = combatCardSelection.NextItem(cards.Where((Func<CardModel, bool>) (c => c.CostsEnergyOrStars(false)))) ??
                                  combatCardSelection.NextItem(cards.Where((Func<CardModel, bool>) (c => c.CostsEnergyOrStars(true))));
            if (cardModel != null)
            {
                cardModel.SetToFreeThisTurn();
                NCard.FindOnTable(cardModel)?.CardHighlight.AnimFlash();
                Flash();
            }
        }
    }
}