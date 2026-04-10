using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class CardPileCmdPatch
    {
        public interface IDrawCardListener
        {
            Task AfterDrawCardFinish(PlayerChoiceContext choiceContext,
                decimal count,
                Player player,
                bool fromHandDraw);
        }
        [HarmonyPatch(typeof(CardPileCmd),nameof(CardPileCmd.Draw),[typeof(PlayerChoiceContext) ,
            typeof(decimal) ,
            typeof(Player) ,
            typeof(bool)])]
        public static class DrawCardPatch
        {
            [HarmonyPostfix]
            public static async Task<IEnumerable<CardModel>> Postfix(Task<IEnumerable<CardModel>> __result,PlayerChoiceContext choiceContext,
                decimal count,
                Player player,
                bool fromHandDraw)
            {
                var r=await __result;
                CombatState combatState = player.Creature.CombatState;
                if (combatState != null)
                {
                    foreach (AbstractModel i in combatState.IterateHookListeners())
                    {
                        if (i is IDrawCardListener listener)
                        {
                            await listener.AfterDrawCardFinish(choiceContext, count, player, fromHandDraw);
                        }
                    }
                }
                return r;
            }
        }
    }
}