using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class AttackCommandPatch
    {
        public interface IModifyAttackCommandTargets
        {
            List<Creature> ModifyAttackTargets(List<Creature> origin,AttackCommand command);
        }
        [HarmonyPatch(typeof(AttackCommand),nameof(AttackCommand.Execute))]
        [HarmonyPatch(MethodType.Async)]
        static class ModifyAttackTargetsPatch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return (List<CodeInstruction>)new InstructionPatcher(instructions)
                    .MatchStart().Match(new InstructionMatcher().ldarg_0().ldfld(null).PredicateMatch(o =>
                    {
                        FieldInfo f=o as FieldInfo;
                        return f!=null && f.Name.Contains("this");
                    })).CopyMatch(out List<CodeInstruction> @this)
                    .MatchStart().Match(new CallMatcher(AccessTools.Method(typeof(AttackCommand), "GetPossibleTargets")))
                    .Insert(@this)
                    .Insert(CodeInstruction.Call(typeof(ModifyAttackTargetsPatch), nameof(Modify)));
            }

            public static IReadOnlyList<Creature> Modify(IReadOnlyList<Creature> origin, AttackCommand command)
            {
                ICombatState combatState = command.Attacker?.CombatState;
                if (combatState == null) return origin;
                List<Creature> list = origin.ToList();
                foreach (AbstractModel iterateHookListener in combatState.IterateHookListeners())
                {
                    if (iterateHookListener is IModifyAttackCommandTargets model)
                    {
                        model.ModifyAttackTargets(list, command);
                    }
                }
                return list;
            }
        }
    }
}