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
                bool alreadyFind = false;
                foreach (Player p in __instance.Players)
                {
                    if (!alreadyFind&&p.Creature.CombatState!=null&&p.Character is Character.HakureiReimu c)
                    {
                        alreadyFind = true;
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
                                if (!orb.HasBeenRemovedFromState && orb.Owner.IsActiveForHooks)
                                {
                                    yield return orb;
                                }
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