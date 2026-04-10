using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.PersistCard.Patch
{
    public class CardPilePatch
    {
        [HarmonyPatch(typeof(CardPile),nameof(CardPile.IsCombatPile),MethodType.Getter)]
        public static class IsCombatPilePatch
        {
            [HarmonyPrefix]
            public static bool Prefix(CardPile __instance,ref bool __result)
            {
                if (__instance is AbstractPersistCardTable cardTable)
                {
                    __result = cardTable.IsCombatPile;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CardModel), nameof(CardModel.UpdateDynamicVarPreview), [typeof(CardPreviewMode),
            typeof(Creature),
            typeof(DynamicVarSet)])]
        public static class UpdateDynamicVarPreviewPatch
        {
            // [HarmonyTranspiler]
            // public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,ILGenerator ilGenerator)
            // {
            //     var label = ilGenerator.DefineLabel();
            //     return (List<CodeInstruction>)new InstructionPatcher(instructions)
            //         .Match((new InstructionMatcher()).ldloc_2().stloc_0())
            //         .Insert([
            //             new CodeInstruction(OpCodes.Ldarg_0),
            //             new CodeInstruction(OpCodes.Call,AccessTools.PropertyGetter(typeof(CardModel),nameof(CardModel.Pile))),
            //             new CodeInstruction(OpCodes.Isinst,typeof(AbstractPersistCardTable)),
            //             new CodeInstruction(OpCodes.Dup),
            //             new CodeInstruction(OpCodes.Brfalse_S,label),
            //             new CodeInstruction(OpCodes.Dup),
            //             new CodeInstruction(
            //                 OpCodes.Callvirt,
            //                 AccessTools.PropertyGetter(typeof(AbstractPersistCardTable),
            //                     nameof(AbstractPersistCardTable.AlwaysShowsDynamicVarPreview))
            //             ),
            //             new CodeInstruction(OpCodes.Stloc_0),
            //             new CodeInstruction(OpCodes.Pop).WithLabels(label),
            //         ]);
            // }
            [HarmonyPostfix]
            public static void Postfix(CardModel __instance, CardPreviewMode previewMode,
                Creature target,
                DynamicVarSet dynamicVarSet)
            {
                if (__instance.RunState == null && __instance.CombatState == null)
                    return;
                if (__instance.CombatState != null && (__instance.UpgradePreviewType == CardUpgradePreviewType.Combat ||
                    __instance.Pile is AbstractPersistCardTable { AlwaysShowsDynamicVarPreview: true })) 
                {
                    foreach (DynamicVar dynamicVar in (IEnumerable<DynamicVar>) dynamicVarSet.Values.ToList())
                        dynamicVar.UpdateCardPreview(__instance, previewMode, target, true);
                }
            }
        }
    }
}