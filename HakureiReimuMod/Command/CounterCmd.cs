using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HakureiReimu.HakureiReimuMod.Command
{
    public static class CounterCmd
    {
        public static async Task InvokeCounter(CombatState state,ICounter counter,Creature target, bool cost = true, bool instant = false)
        {
            if (state == null || counter == null)
            {
                MainFile.Logger.Warn("尝试发动错误的反制:"+nameof(state)+":"+state+"  "+nameof(counter)+":"+counter);
                return;
            }
            CounterManager.InInvokeCounter = true;
            await CounterManager.BeforeCounter(state,counter,target);
            await counter.Invoke(target, cost, instant);
            await CounterManager.AfterCounter(state,counter,target);
            CounterManager.InInvokeCounter = false;
        }
        public static async Task InvokeCounter(CombatState state,IEnumerable<ICounter> counters,Creature target, bool cost = true, bool instant = false)
        {
            foreach (ICounter counter in counters)
            {
                await InvokeCounter(state,counter,target,cost,instant);
            }
        }
    }
}