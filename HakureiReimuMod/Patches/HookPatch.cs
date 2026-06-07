using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public static class HookPatch
    {
        public interface IModifyPowerAmountGivenFinal
        {
            public decimal ModifyPowerAmountGivenFinal(PowerModel power, Creature giver, decimal amount, Creature target, CardModel cardSource);
        }
        [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyPowerAmountGiven))]
        public static class HookModifyPowerAmountGivenFinalPatch
        {
            [HarmonyPostfix]
            public static void Postfix(ref decimal __result, ICombatState combatState,
                PowerModel power,
                Creature giver,
                decimal amount,
                Creature target,
                CardModel cardSource)
            {
                decimal d = __result;
                if (!CombatManager.Instance.IsOverOrEnding || CombatManager.Instance.IsStarting)
                {
                    foreach (AbstractModel listener in combatState.IterateHookListeners())
                    {
                        if (listener is IModifyPowerAmountGivenFinal modifier)
                        {
                            d = modifier.ModifyPowerAmountGivenFinal(power, giver, d, target, cardSource);
                        }
                    }
                }
                __result = d;
            }
        }
    }
}