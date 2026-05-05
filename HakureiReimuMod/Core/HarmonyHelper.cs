using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public static class HarmonyHelper
    {
        [HarmonyPatch(typeof(OneTimeInitialization), nameof(OneTimeInitialization.ExecuteEssential))]
        static class InitPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                Harmony harmony = new(typeof(InitPatch).FullName);
                HakureiReimuMain.AfterGameInit(harmony);
            }
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
            HakureiReimuMain.Logger.Info("Patched DamageProps:"+type.Name+":"+method.Name);
            afterSuccess?.Invoke(harmony,type);
        }
    }
}