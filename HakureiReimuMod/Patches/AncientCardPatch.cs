using System.Collections.Generic;
using HakureiReimu.HakureiReimuMod.Cards.Attack.Common;
using HakureiReimu.HakureiReimuMod.Cards.Attack.Rare;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class AncientCardPatch
    {
        [HarmonyPatch(typeof(ArchaicTooth), "TranscendenceUpgrades",MethodType.Getter)]
        public static class ArchaicToothPatch
        {
            [HarmonyPostfix]
            public static void Postfix(ArchaicTooth __instance,Dictionary<ModelId, CardModel> __result)
            {
                __result.Add(ModelDb.Card<Seal>().Id,ModelDb.Card<DreamSealingDivine>());
            }
        }
    }
}