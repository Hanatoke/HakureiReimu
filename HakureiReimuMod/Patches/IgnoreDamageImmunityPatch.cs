using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BaseLib.Patches.Content;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class IgnoreDamageImmunityPatch
    {
        [HarmonyPatch(typeof(OneTimeInitialization), nameof(OneTimeInitialization.ExecuteEssential))]
        public static class InitPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                Harmony harmony = new(typeof(InitPatch).FullName);
                PatchAll(harmony,ModelDb.AllPowers.Select(p => p.GetType()).ToList());
            }
        }
        [CustomEnum]
        public static ValueProp IgnoreDamageImmunity;
        public static void PatchAll(Harmony harmony,IEnumerable<Type> types)
        {
            List<(string, MethodInfo, MethodInfo, MethodInfo,Action<Harmony,Type>)> patches = new();
            patches.Add(("ModifyDamageCap",GetPatch(nameof(CapPrefix)),null,null,null));
            patches.Add(("ModifyHpLostBeforeOsty",GetPatch(nameof(ModifyHpLostPrefix)),null,null,null));
            patches.Add(("ModifyHpLostBeforeOstyLate",GetPatch(nameof(ModifyHpLostPrefix)),null,null,null));
            patches.Add(("ModifyHpLostAfterOsty",GetPatch(nameof(ModifyHpLostPrefix)),null,null,null));
            patches.Add(("ModifyHpLostAfterOstyLate",GetPatch(nameof(ModifyHpLostPrefix)),null,null,null));
            foreach (Type type in types)
            {
                if (!type.IsClass|| type.IsAbstract)continue;
                foreach (var patch in patches)
                {
                    
                    Patch(harmony,type,patch.Item1,patch.Item2,patch.Item3,patch.Item4,patch.Item5);
                }
            }
            //Special
            Patch(harmony,typeof(HardenedShellPower),nameof(HardenedShellPower.AfterDamageReceived),GetPatch(nameof(DamageTaskPrefix)));
        }

        public static void Patch(Harmony harmony,Type type,string methodName, MethodInfo prefix=null, MethodInfo postfix=null, MethodInfo transpiler=null,
            Action<Harmony,Type> afterSuccess=null)
        {
            MethodInfo method = type.GetMethod(methodName,BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)return;
            if (!method.IsVirtual)return;
            if (method.DeclaringType != type) return;
            if (method.GetBaseDefinition().DeclaringType == type) return;
            harmony.Patch(method, prefix != null ? new HarmonyMethod(prefix) : null,
                postfix != null ? new HarmonyMethod(postfix) : null,
                transpiler != null ? new HarmonyMethod(transpiler) : null);
            HakureiReimuMain.Logger.Info("Patched IgnoreDamageImmunity:"+type.Name+":"+method.Name);
            afterSuccess?.Invoke(harmony,type);
        }
        private static MethodInfo GetPatch(string name)=>typeof(IgnoreDamageImmunityPatch).GetMethod(name, 
            BindingFlags.Public | BindingFlags.Static);

        public static bool CapPrefix(ValueProp props,ref decimal __result)
        {
            if (props.HasFlag(IgnoreDamageImmunity))
            {
                __result = Decimal.MaxValue;
                return false;
            }
            return true;
        }

        public static bool ModifyHpLostPrefix(ValueProp props, decimal amount, ref decimal __result)
        {
            if (props.HasFlag(IgnoreDamageImmunity))
            {
                __result = amount;
                return false;
            }
            return true;
        }

        public static bool DamageTaskPrefix(ValueProp props, ref Task __result)
        {
            if (props.HasFlag(IgnoreDamageImmunity))
            {
                __result = Task.CompletedTask;
                return false;
            }
            return true;
        }
    }
}