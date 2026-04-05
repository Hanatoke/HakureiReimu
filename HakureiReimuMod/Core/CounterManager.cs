using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using HakureiReimu.HakureiReimuMod.Interface.Counter.Hook;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public static class CounterManager
    {
        //TODO:废弃功能,等待测试能否删除
        public static bool InMonsterMove=false;
        public static readonly Dictionary<CardModel, Func<Task>> Later = new();
        public static bool AlreadyForceAttackAnimation=false;
        public static void AddToLater(CardModel card, Func<Task> func)
        {
            Later.TryAdd(card, func);
        }

        public static void CancelLater(CardModel card)
        {
            if (Later.ContainsKey(card))
            {
                Later[card] = null;
            }
        }
        public static async Task RunLater()
        {
            foreach (Func<Task> task in Later.Values)
            {
                if (task!=null)
                {
                    await task();
                }
            }
            Later.Clear();
            AlreadyForceAttackAnimation = false;
        }

        public static async Task ForceAttackAnimation(AttackCommand command)
        {
            if (!AlreadyForceAttackAnimation&&command.Attacker!=null)
            {
                AlreadyForceAttackAnimation=true;
                Traverse traverse = Traverse.Create(command);
                if (traverse.Field<string>("_attackerAnimName").Value!=null&&traverse.Field<bool>("_shouldPlayAnimation").Value)
                {
                    await CreatureCmd.TriggerAnim(traverse.Field<Creature>("_visualAttacker").Value ?? command.Attacker
                        , traverse.Field<string>("_attackerAnimName").Value,
                        traverse.Field<float>("_attackerAnimDelay").Value);
                }
            }
        }

        public static async Task BeforeCounter(CombatState state,CardModel card, ICounter counter)
        {
            foreach (AbstractModel l in state.IterateHookListeners())
            {
                if (l is ICounterListener c)
                {
                    await c.BeforeCounter(card, counter);
                }
            }
        }

        public static async Task AfterCounter(CombatState state,CardModel card, ICounter counter)
        {
            foreach (AbstractModel l in state.IterateHookListeners())
            {
                if (l is ICounterListener c)
                {
                    await c.AfterCounter(card, counter);
                }
            }
        }
    }
}