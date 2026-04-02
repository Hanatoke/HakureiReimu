using BaseLib.Utils;
using HakureiReimu.HakureiReimuMod.Core;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class YinYangOrbPatch
    {
        public static SpireField<PlayerCombatState,YinYangOrbManager> Managers =new(_=>null);
        [HarmonyPatch(typeof(PlayerCombatState),MethodType.Constructor,[typeof(Player)])]
        public static class PlayerCombatStateInitPatch
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerCombatState __instance,Player player)
            {
                // Managers[__instance] = new YinYangOrbManager(player);
                Managers[__instance] = ModelDb.GetById<YinYangOrbManager>(ModelDb.GetId<YinYangOrbManager>()).MutableClone() as YinYangOrbManager;
                Managers[__instance].Player = player;
                Managers[__instance].Clear();
            }
        }
        [HarmonyPatch(typeof(PlayerCombatState),nameof(PlayerCombatState.AfterCombatEnd))]
        public static class PlayerCombatStateAfterCombatEndPatch
        {
            [HarmonyPostfix]
            public static void Postfix(PlayerCombatState __instance)
            {
                Managers[__instance]?.Clear();
            }
        }
    }
}