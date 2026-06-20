using System;
using System.Linq;
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

        public static void Patch(Harmony harmony, Type type, MethodInfo targetMethod, MethodInfo prefix = null,
            MethodInfo postfix = null, MethodInfo transpiler = null,
            Action<Harmony, Type> afterSuccess = null)
        {
            try
            {
                MethodInfo method = type.GetMethod(targetMethod.Name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,null,
                    targetMethod.GetParameters().Select(i => i.ParameterType).ToArray(),null);
                if (method == null) return;
                if (!method.IsVirtual) return;
                if (method.DeclaringType != type) return;
                if (method.GetBaseDefinition().DeclaringType == type) return;
                harmony.Patch(method, prefix != null ? new HarmonyMethod(prefix) : null,
                    postfix != null ? new HarmonyMethod(postfix) : null,
                    transpiler != null ? new HarmonyMethod(transpiler) : null);
                HakureiReimuMain.Logger.Info("Patched override:" + type.Name + ":" + method.Name);
                afterSuccess?.Invoke(harmony, type);
            }
            catch (Exception e)
            {
                HakureiReimuMain.Logger.Info("Patch Error:" + type.Name + ":" + targetMethod+" skipped!");
                throw;
            }
        }
    }
}