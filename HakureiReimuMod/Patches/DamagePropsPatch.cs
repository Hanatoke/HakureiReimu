using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public static class DamagePropsPatch
    {
        /// <summary>
        /// 无视伤害减免(飞行,难以杀灭,灵体,坚不可摧...)
        /// </summary>
        public static readonly ValueProp IgnoreDamageImmunity=(ValueProp)(1<<17);
        /// <summary>
        /// 无视伤害响应(荆棘)
        /// </summary>
        public static readonly ValueProp IgnoreDamageResponse=(ValueProp)(1<<18);
        public static void PatchAll(Harmony harmony,IEnumerable<Type> types)
        {
            List<(string, MethodInfo, MethodInfo, MethodInfo,Action<Harmony,Type>)> patches = new();
            patches.Add(("ModifyDamageCap",GetInfo(typeof(IgnoreDamageImmunityPatch),nameof(IgnoreDamageImmunityPatch.CapPrefix)),null,null,null));
            patches.Add(("ModifyHpLostBeforeOsty",GetInfo(typeof(IgnoreDamageImmunityPatch),nameof(IgnoreDamageImmunityPatch.ModifyHpLostPrefix)),null,null,null));
            patches.Add(("ModifyHpLostBeforeOstyLate",GetInfo(typeof(IgnoreDamageImmunityPatch),nameof(IgnoreDamageImmunityPatch.ModifyHpLostPrefix)),null,null,null));
            patches.Add(("ModifyHpLostAfterOsty",GetInfo(typeof(IgnoreDamageImmunityPatch),nameof(IgnoreDamageImmunityPatch.ModifyHpLostPrefix)),null,null,null));
            patches.Add(("ModifyHpLostAfterOstyLate",GetInfo(typeof(IgnoreDamageImmunityPatch),nameof(IgnoreDamageImmunityPatch.ModifyHpLostPrefix)),null,null,null));
            foreach (Type type in types)
            {
                if (!type.IsClass|| type.IsAbstract)continue;
                foreach (var patch in patches)
                {
                    HarmonyHelper.Patch(harmony,type,patch.Item1,patch.Item2,patch.Item3,patch.Item4,patch.Item5);
                }
            }
            //Special
            HarmonyHelper.Patch(harmony,typeof(HardenedShellPower),nameof(HardenedShellPower.AfterDamageReceived),GetInfo(typeof(IgnoreDamageImmunityPatch),nameof(IgnoreDamageImmunityPatch.DamageTaskPrefix)));
            HarmonyHelper.Patch(harmony,typeof(SoarPower),nameof(SoarPower.ModifyDamageMultiplicative),GetInfo(typeof(IgnoreDamageImmunityPatch),nameof(IgnoreDamageImmunityPatch.MultiplicativePrefix)));
            HarmonyHelper.Patch(harmony,typeof(FlutterPower),nameof(FlutterPower.ModifyDamageMultiplicative),GetInfo(typeof(IgnoreDamageImmunityPatch),nameof(IgnoreDamageImmunityPatch.MultiplicativePrefix)));
            
            HarmonyHelper.Patch(harmony,typeof(ThornsPower),nameof(ThornsPower.BeforeDamageReceived),GetInfo(typeof(IgnoreDamageResponsePatch),nameof(IgnoreDamageResponsePatch.DamageTaskPrefix)));
        }
        
        private static MethodInfo GetInfo(Type type,string name)=>type.GetMethod(name, 
            BindingFlags.Public | BindingFlags.Static);
        public static class IgnoreDamageImmunityPatch
        {
            public static bool MultiplicativePrefix(ValueProp props, ref decimal __result)
            {
                if (props.HasFlag(IgnoreDamageImmunity))
                {
                    __result = 1m;
                    return false;
                }
                return true;
            }
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
            [HarmonyPriority(Priority.Last)]
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
        public static class IgnoreDamageResponsePatch
        {
            [HarmonyPriority(Priority.Last)]
            public static bool DamageTaskPrefix(ValueProp props, ref Task __result)
            {
                if (props.HasFlag(IgnoreDamageResponse))
                {
                    __result = Task.CompletedTask;
                    return false;
                }
                return true;
            }
        }
    }
}