using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class CounterPatch
    {
        [HarmonyPatch(typeof(MonsterModel),nameof(MonsterModel.PerformMove))]
        public static class MonsterPerformMovePatch
        {
            // [HarmonyPrefix]
            // public static bool Prefix(MonsterModel __instance)
            // {
            //     CounterManager.InMonsterMove = true;
            //     return true;
            // }
            [HarmonyPostfix]
            public static async Task Postfix(Task __result,MonsterModel __instance)
            {
                CounterManager.InMonsterMove = true;
                await __result;
                CounterManager.InMonsterMove = false;
                await CounterManager.RunLater();
            }
        }
    }
}