using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class CardPileCmdPatch
    {
        public interface IDrawCardListener
        {
            Task BeforeDrawCardStart(PlayerChoiceContext choiceContext,
                decimal count,
                Player player,
                bool fromHandDraw){return Task.CompletedTask;}
            Task AfterDrawCardFinish(PlayerChoiceContext choiceContext,
                decimal count,
                Player player,
                bool fromHandDraw){return Task.CompletedTask;}
        }
        [HarmonyPatch(typeof(CardPileCmd),nameof(CardPileCmd.Draw),[typeof(PlayerChoiceContext) ,
            typeof(decimal) ,
            typeof(Player) ,
            typeof(bool)])]
        public static class DrawCardPatch
        {
            [HarmonyTranspiler][HarmonyPatch(MethodType.Async)]
            public static IEnumerable<CodeInstruction> BeforeTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                return AsyncMethodCall.Create(generator, instructions, original,
                    AccessTools.Method(typeof(DrawCardPatch), nameof(Before)), beforeState: original);
            }
            // 暂时找不到正确的state
            // [HarmonyTranspiler]
            // public static IEnumerable<CodeInstruction> AfterTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions, MethodBase original)
            // {
            //     return AsyncMethodCall.Create(generator, instructions, original,
            //         AccessTools.Method(typeof(DrawCardPatch), nameof(After)), afterState: original);
            // }
            [HarmonyPostfix]
            public static async Task<IEnumerable<CardModel>> Postfix(Task<IEnumerable<CardModel>> __result,PlayerChoiceContext choiceContext,
                decimal count,
                Player player,
                bool fromHandDraw)
            {
                IEnumerable<CardModel> r=await __result;
                await After(choiceContext,count,player,fromHandDraw);
                return r;
            }
            public static async Task Before(PlayerChoiceContext choiceContext,
                decimal count,
                Player player,
                bool fromHandDraw)
            {
                ICombatState combatState = player.Creature.CombatState;
                if (combatState != null)
                {
                    foreach (AbstractModel i in combatState.IterateHookListeners())
                    {
                        if (i is IDrawCardListener listener)
                        {
                            await listener.BeforeDrawCardStart(choiceContext, count, player, fromHandDraw);
                        }
                    }
                }
            }
            public static async Task After(PlayerChoiceContext choiceContext,
                decimal count,
                Player player,
                bool fromHandDraw)
            {
                ICombatState combatState = player.Creature.CombatState;
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
            }
        }
    }
}