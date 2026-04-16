using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class SealPatch
    {
        public static bool IsInRealDamage=false;
        [HarmonyPatch(typeof(CreatureCmd),nameof(CreatureCmd.Damage),typeof(PlayerChoiceContext),typeof(IEnumerable<Creature>),typeof(decimal),typeof(ValueProp),
            typeof(Creature),typeof(CardModel))]
        public static class DamageCmdPatch
        {
            // [HarmonyPrefix]
            // public static bool Prefix(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets,
            //     ref decimal amount, ref ValueProp props, Creature dealer, CardModel cardSource)
            // {
            //     if (dealer!=null && dealer.IsAlive)
            //     {
            //         IEnumerable<Creature> t = targets.ToList();
            //         foreach (PowerModel p in dealer.Powers.ToList())
            //         {
            //             if (p is SealPower seal)
            //             {
            //                 seal.ModifyDamages(ref amount, ref props,dealer,cardSource,t);
            //             }
            //         }
            //     }
            //     return true;
            // }
            [HarmonyPrefix]
            public static bool Prefix()
            {
                IsInRealDamage = true;
                return true;
            }

            [HarmonyPostfix]
            public static void Postfix()
            {
                IsInRealDamage=false;
            }
        }
        [HarmonyPatch(typeof(Hook),nameof(Hook.ModifyDamage))]
        public static class DamageHookPatch
        {
            [HarmonyPostfix,HarmonyPriority(Priority.Last)]
            public static void Postfix(ref decimal __result,IRunState runState,
                CombatState? combatState,
                Creature? target,
                Creature? dealer,
                decimal damage,
                ValueProp props,
                CardModel? cardSource)
            {
                if (IsInRealDamage&& dealer != null && dealer.IsAlive) 
                {
                    foreach (PowerModel p in dealer.Powers.ToList())
                    {
                        if (p is SealPower seal)
                        {
                            seal.ModifyDamage(ref __result, ref props,dealer, cardSource,target);
                        }
                    }
                }
            }
        }
    }
}