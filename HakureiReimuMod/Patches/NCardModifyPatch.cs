using System;
using System.Reflection;
using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Interface;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Pooling;
namespace HakureiReimu.HakureiReimuMod.Patches
{
    public class NCardModifyPatch
    {
        [HarmonyPatch(typeof(NCard),"Reload")]
        public static class ReloadPatch
        {
            [HarmonyPostfix]
            public static void Postfix(NCard __instance)
            {
                if (!__instance.IsNodeReady())
                {
                    return;
                }
                if (__instance.Model is INCardModify nCardCreate)
                {
                    nCardCreate.OnReload(__instance);
                }
            }
        }
        //-----------------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(GodotTreeExtensions),nameof(GodotTreeExtensions.QueueFreeSafely))]
        public static class QueueFreeSafelyPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(Godot.Node node)
            {
                if (GodotObject.IsInstanceValid(node) && node is NCard { Model: INCardModify { AllowNodePool: false } })
                {
                    node.QueueFreeSafelyNoPool();
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(NCard),nameof(NCard.Model), MethodType.Setter)]
        public static class NCardModelSetPatch
        {
            private static readonly MethodInfo UnsubscribeFromModel = AccessTools.Method(typeof(NCard), "UnsubscribeFromModel");
            private static readonly FieldInfo CardOverlay = AccessTools.Field(typeof(NCard), "_cardOverlay");
            [HarmonyPrefix]
            public static bool Prefix(NCard __instance,ref CardModel ____model,CardModel value)
            {
                
                if (____model!=value&&____model is INCardModify modify)
                {
                    try
                    {
                        UnsubscribeFromModel.Invoke(__instance, [____model]);
                        NCard template=NodePool.Get<NCard>();
                        if (template.Body==null)
                        {
                            template._Ready();
                        }
                        Control control = template.Body;
                        Vector2 t=__instance.Body.Position;
                        
                        __instance.Body.Free();
                        CardOverlay.SetValue(__instance,null);
                        
                        control.ReparentSafely(__instance);
                        control.Position = t;
                        ____model = template.Model;
                        template.QueueFreeSafelyNoPool();
                        SetUniqueNameToOwner(control, __instance);
                        
                        __instance._Ready();
                    }
                    catch (Exception e)
                    {
                        HakureiReimuMain.Logger.Info(e.ToString());
                    }
                }
                return true;
            }
        }

        private static void SetUniqueNameToOwner(Godot.Node node, Godot.Node parent)
        {
            node.UniqueNameInOwner = true;
            node.Owner = parent;
            foreach (Godot.Node child in node.GetChildren())
            {
                SetUniqueNameToOwner(child, parent);
            }
        }
    }
}