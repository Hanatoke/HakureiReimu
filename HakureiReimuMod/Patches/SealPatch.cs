using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using BaseLib.Utils.Patching;
using HakureiReimu.HakureiReimuMod.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class SealPatch
    {
        [HarmonyPatch(typeof(CreatureCmd),nameof(CreatureCmd.Damage),[typeof(PlayerChoiceContext),typeof(IEnumerable<Creature>),typeof(decimal),typeof(ValueProp),
            typeof(Creature),typeof(CardModel)])]
        [HarmonyPatch(MethodType.Async)]
        public static class DamageCmdPatch
        {
            [HarmonyTranspiler,HarmonyPriority(Priority.First)]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return (List<CodeInstruction>)new InstructionPatcher(instructions)
                    .MatchStart().Match(new InstructionMatcher().ldarg_0().ldfld(null).PredicateMatch(o =>
                {
                    FieldInfo f=o as FieldInfo;
                    return f!=null && f.Name.Contains("combatState");
                })).CopyMatch(out List<CodeInstruction> combatStateMatch)
                    .MatchStart().Match(new InstructionMatcher().ldarg_0().ldfld(null).PredicateMatch(o =>
                {
                    FieldInfo f=o as FieldInfo;
                    return f!=null && f.Name.Contains("props");
                })).CopyMatch(out List<CodeInstruction> propsMatch)
                    .MatchStart().Match(new InstructionMatcher().ldarg_0().ldfld(null).PredicateMatch(o =>
                {
                    FieldInfo f=o as FieldInfo;
                    return f!=null && f.Name.Contains("dealer");
                })).CopyMatch(out List<CodeInstruction> dealerMatch)
                    .MatchStart().Match(new InstructionMatcher().ldarg_0().ldfld(null).PredicateMatch(o =>
                {
                    FieldInfo f=o as FieldInfo;
                    return f!=null && f.Name.Contains("originalTarget");
                })).CopyMatch(out List<CodeInstruction> originalTargetMatch)
                    .Match(new InstructionMatcher().stfld(null).PredicateMatch(o =>
                {
                    FieldInfo f=o as FieldInfo;
                    return f!=null && f.Name.Contains("modifiedAmount");
                })).Step(-1)
                    .Insert(combatStateMatch)
                    .Insert(propsMatch)
                    .Insert(dealerMatch)
                    .Insert(originalTargetMatch)
                    .Insert(CodeInstruction.Call(typeof(SealPatch),nameof(AfterModifyDamage)))
                    ;
            }
        }

        public static decimal AfterModifyDamage(decimal amount,CombatState state, ValueProp props, Creature dealer, Creature target)
        {
            foreach (AbstractModel l in state.IterateHookListeners())
            {
                if (l is SealPower seal)
                {
                    seal.ModifyDamage(ref amount, props, dealer, target);
                }
            }
            return amount;
        }
    }
}