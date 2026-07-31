using System;
using BaseLib.Utils;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;

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

        public static void SetPreviewTarget(Player player, Creature target)
        {
            // HakureiReimuMain.Logger.Info(target?.Name ?? "null");
            player?.Creature.GetCreatureNode()
                ?.NYinYangOrbManager(player.PlayerCombatState?.YinYangOrbManager())
                ?.OnCardPlayHover(target);
        }
        
        [HarmonyPatch(typeof(NCard), nameof(NCard.SetPreviewTarget))]
        public static class NCardSetPreviewTargetPatch
        {
            [HarmonyPostfix]
            public static void Postfix(NCard __instance,Creature creature)
            {
                try
                {
                    CardModel card = __instance.Model;
                    Player player = card?.IsMutable == true ? card.Owner : null;
                    SetPreviewTarget(player, creature);
                }
                catch (Exception e)
                {
                    HakureiReimuMain.Logger.Warn(e.ToString());
                }
            }
        }

        [HarmonyPatch(typeof(NCardPlay), "ShowMultiCreatureTargetingVisuals")]
        public static class NCardPlayShowMultiCreatureTargetingVisualsPatch
        {
            [HarmonyPostfix]
            public static void Postfix(NCardPlay __instance)
            {
                //用于修复某个神秘bug，可能是JIT优化导致的
            }
        }
        [HarmonyPatch(typeof(NCardPlay), "OnCreatureHover")]
        public static class NCardPlayOnCreatureHoverPatch
        {
            [HarmonyPostfix]
            public static void Postfix(NCardPlay __instance)
            {
                //用于修复某个神秘bug，可能是JIT优化导致的
            }
        }
        [HarmonyPatch(typeof(NCardPlay), "OnCreatureUnhover")]
        public static class NCardPlayOnCreatureUnhoverPatch
        {
            [HarmonyPostfix]
            public static void Postfix(NCardPlay __instance)
            {
                //用于修复某个神秘bug，可能是JIT优化导致的
            }
        }

        [HarmonyPatch(typeof(NCardPlay), "HideTargetingVisuals")]
        public static class NCardPlayHideTargetingVisualsPatch
        {
            [HarmonyPostfix]
            public static void Postfix(NCardPlay __instance)
            {
                SetPreviewTarget(__instance.Player,null);
            }
        }
    }
}