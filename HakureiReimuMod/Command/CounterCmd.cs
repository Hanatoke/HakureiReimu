using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Command
{
    public static class CounterCmd
    {
        public static async Task InvokeCounter(CombatState state,ICounter counter,Creature target, bool cost = true, bool instant = false)
        {
            await CounterManager.BeforeCounter(state,counter);
            await counter.Invoke(target, cost, instant);
            await CounterManager.AfterCounter(state,counter);
        }
    }
}