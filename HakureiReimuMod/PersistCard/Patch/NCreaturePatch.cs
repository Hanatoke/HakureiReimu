using System.Threading;
using HakureiReimu.HakureiReimuMod.PersistCard.Extensions;
using HakureiReimu.HakureiReimuMod.PersistCard.Node;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace HakureiReimu.HakureiReimuMod.PersistCard.Patch
{
    public class NCreaturePatch
    {
        [HarmonyPatch(typeof(NCreature),"OnCombatEnded")]
        public static class CombatEndPatch
        {
            [HarmonyPostfix]
            public static void Postfix(NCreature __instance)
            {
                __instance.ClearPersistCardTable();
            }
        }
        [HarmonyPatch(typeof(NCreature),"AnimDie",[typeof(bool),typeof(CancellationToken)])]
        public static class AnimDiePatch
        {
            [HarmonyPostfix]
            public static void Postfix(NCreature __instance, bool shouldRemove, CancellationToken cancelToken)
            {
                if (!RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
                {
                    __instance.ClearPersistCardTable();
                }
            }
        }
    }
}