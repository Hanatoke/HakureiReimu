using System.Collections.Generic;
using HakureiReimu.HakureiReimuMod.Core;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class CombatStatePatch
    {
        [HarmonyPatch(typeof(CombatState),methodName:nameof(CombatState.IterateHookListeners))]
        public static class CharacterIterateHookPatch
        {
            [HarmonyPostfix]
            public static void Postfix(CombatState __instance,ref IEnumerable<AbstractModel>  __result)
            {
                //TODO:在新版本中移除
                List<AbstractModel> list = new(__result);
                foreach (Player p in __instance.Players)
                {
                    if (p.Character is Character.HakureiReimu c)
                    {
                        list.Insert(0, c);
                    }

                    if (p.PlayerCombatState != null)
                    {
                        YinYangOrbManager m=YinYangOrbPatch.Managers[p.PlayerCombatState];
                        if (m != null)
                        {
                            list.Add(m);
                            list.AddRange(m.Orbs);
                        }
                    }
                }
                __result = list;
            }
        }
    }
}