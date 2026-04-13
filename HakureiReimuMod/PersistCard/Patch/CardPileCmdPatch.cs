using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using HakureiReimu.HakureiReimuMod.PersistCard.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.PersistCard.Patch
{
    public class CardPileCmdPatch
    {
        [HarmonyPatch(typeof(CardPileCmd),nameof(CardPileCmd.Add),[typeof(IEnumerable<CardModel>),
            typeof(CardPile),
            typeof(CardPilePosition),
            typeof(AbstractModel),
            typeof(bool)])]
        public static class AddCardPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(IEnumerable<CardModel> cards,
                CardPile newPile,
                CardPilePosition position ,
                AbstractModel source ,
                bool skipVisuals,ref Task<IReadOnlyList<CardPileAddResult>> __result)
            {
                if (newPile is AbstractPersistCardTable table)
                {

                    __result = Replace(cards, table);
                    return false;
                }
                return true;
            }

            private static async Task<IReadOnlyList<CardPileAddResult>> Replace(IEnumerable<CardModel> cards, AbstractPersistCardTable table)
            {
                List<CardModel> list = new (cards);
                if (CombatManager.Instance.IsEnding)
                {
                    return  list.Select((c => new CardPileAddResult()
                    {
                        cardAdded = c,
                        success = false
                    })).ToList();
                }
                List<CardPileAddResult> results = new();
                foreach (CardModel card in list)
                {
                    results.Add(new CardPileAddResult()
                    {
                        cardAdded = card,
                        success = true,
                        oldPile = card.Pile
                    });
                    AbstractPersistCardSlot slot = card.GetInstanceSlot();
                    
                    await PersistCardCmd.StartPersistCard(table, slot);
                    // await Hook.AfterCardChangedPiles(card.RunState, card.CombatState, card, card.Pile.Type, null);
                }
                return results;
            }
        }
        [HarmonyPatch(typeof(Hook),nameof(Hook.AfterCardPlayed))]
        public static class AfterCardPlayPatch
        {
            [HarmonyPostfix]
            public static async Task Postfix(Task __result, CombatState combatState,
                PlayerChoiceContext choiceContext,
                CardPlay cardPlay)
            {
                await  __result;
                if (cardPlay?.Card.Owner.PlayerCombatState==null)return;
                if (cardPlay.PlayIndex>0&&cardPlay.PlayCount>1&&cardPlay.ResultPile.GetPile(cardPlay.Card.Owner) is AbstractPersistCardTable table)
                {
                    CardModel card = cardPlay.Card.CreateDupe();
                    card.AddKeyword(CardKeyword.Exhaust);
                    await CardPileCmd.Add(card, PileType.Play, skipVisuals: true);
                    await PersistCardCmd.StartPersistCard(table,card.GetInstanceSlot());
                }
            }
        }
    }
}