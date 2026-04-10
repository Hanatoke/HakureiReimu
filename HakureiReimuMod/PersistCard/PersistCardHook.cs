using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.PersistCard.Interface;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.PersistCard
{
    public static class PersistCardHook
    {
        public static async Task OnStartPersistCard(CombatState state,AbstractPersistCardSlot slot)
        {
            await slot.OnStart();
            foreach (AbstractModel m in state.IterateHookListeners())
            {
                if (m is IPersistCardSubscriber s)
                {
                    await s.OnStartPersistCard(slot);
                }
            }
            foreach (AbstractModel m in state.IterateHookListeners())
            {
                if (m is IPersistCardSubscriber s)
                {
                    await s.OnStartPersistCardLater(slot);
                }
            }
        }

        public static async Task OnStopPersistCard(CombatState combatState, AbstractPersistCardSlot slot)
        {
            await slot.OnEnd();
            foreach (AbstractModel m in combatState.IterateHookListeners())
            {
                if (m is IPersistCardSubscriber s)
                {
                    await s.OnStopPersistCard(slot);
                }
            }
            foreach (AbstractModel m in combatState.IterateHookListeners())
            {
                if (m is IPersistCardSubscriber s)
                {
                    await s.OnStartPersistCardLater(slot);
                }
            }
        }

        public static bool AtIncreaseCount(CombatState state, AbstractPersistCardSlot slot,int origin,ref int result)
        {
            bool r = true;
            foreach (AbstractModel m in state.IterateHookListeners())
            {
                if (m is IPersistCardSubscriber s)
                {
                    r=s.AtIncreasePersistCount(slot, origin, ref result);
                }
            }
            return r;
        }

        public static bool AtDecreaseCount(CombatState state, AbstractPersistCardSlot slot,int origin,ref int result)
        {
            bool r = true;
            foreach (AbstractModel m in state.IterateHookListeners())
            {
                if (m is IPersistCardSubscriber s)
                {
                    r=s.AtDecreasePersistCount(slot, origin, ref result);
                }
            }
            return r;
        }

        public static async Task AfterModifyPersistCount(CombatState combatState, AbstractPersistCardSlot slot,int result)
        {
            foreach (AbstractModel m in combatState.IterateHookListeners())
            {
                if (m is IPersistCardSubscriber s)
                {
                    await s.AfterModifyPersistCount(slot,result);
                }
            }
        }
    }
}