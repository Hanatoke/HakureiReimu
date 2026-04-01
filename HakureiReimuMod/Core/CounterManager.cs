using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using HakureiReimu.HakureiReimuMod.Interface.Counter.Hook;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public static class CounterManager
    {
        public static bool InMonsterMove=false;
        public static readonly Dictionary<CardModel, Func<Task>> Later = new();
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