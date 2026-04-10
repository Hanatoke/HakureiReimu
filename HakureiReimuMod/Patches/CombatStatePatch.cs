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
            public static IEnumerable<AbstractModel> Postfix(IEnumerable<AbstractModel>  __result,CombatState __instance)
            {
                //TODO:在新版本中移除
                foreach (Player p in __instance.Players)
                {
                    if (p.Character is Character.HakureiReimu c)
                    {
                        yield return c;
                    }
                
                    if (p.PlayerCombatState != null)
                    {
                        YinYangOrbManager m=YinYangOrbPatch.Managers[p.PlayerCombatState];
                        if (m != null)
                        {
                            yield return m;
                            foreach (YinYangOrb orb in m.Orbs)
                            {
                                yield return orb;
                            }
                        }
                    }
                }
                foreach (AbstractModel abstractModel in __result)
                {
                    yield return abstractModel;
                }
            }
        }
    }
}