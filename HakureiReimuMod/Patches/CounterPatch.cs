using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

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
        //TODO:在未来有公开接口后替换
        [HarmonyPatch(typeof(ThornsPower),nameof(ThornsPower.BeforeDamageReceived))]
        public static class IgnoreThornsPatch
        {
            [HarmonyPostfix]
            public static async Task Postfix(Task __result,ThornsPower __instance,CardModel cardSource)
            {
                if (cardSource!=null&&cardSource.HasCounter())
                {
                    return;
                }
                await __result;
            }
        }
    }
}