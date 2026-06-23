using System;
using System.Collections.Generic;
using System.Linq;
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
        public static readonly Dictionary<Godot.Node, (List<NodePath>, List<NodePath>)> HasModify = new();
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

                if (__instance.Model is INCardModify nCardCreate && !HasModify.ContainsKey(__instance))
                {
                    List<Godot.Node> needRecover = [];
                    List<Godot.Node> needRemove = [];
                    nCardCreate.OnReload(__instance, needRecover, needRemove);
                    if (needRecover.Count>0|| needRemove.Count>0)
                    {
                        HasModify[__instance] = (needRecover.Select(n => __instance.GetPathTo(n)).ToList(),
                            needRemove.Select(n => __instance.GetPathTo(n)).ToList());
                    }
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
                if (GodotObject.IsInstanceValid(node) && node is NCard card &&  HasModify.ContainsKey(card))
                {
                    HasModify.Remove(node);
                    node.QueueFreeSafelyNoPool();
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(NCard), nameof(NCard.Model), MethodType.Setter)]
        public static class ModelSetterPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(NCard __instance, ref CardModel ____model, CardModel value)
            {
                if (____model != value && HasModify.ContainsKey(__instance))
                {
                    if (!TryRecoverNCard(__instance))
                    {
                        HasModify.Remove(__instance);
                        OldNCardModelSetPatch.Prefix(__instance,ref ____model, value);
                    }
                }

                return true;
            }
        }
        public static readonly FieldInfo ModelField = AccessTools.Field(typeof(NCard), "_model");

        public static bool TryRecoverNCard(NCard card)
        {
            try
            {
                if (!HasModify.TryGetValue(card, out (List<NodePath>, List<NodePath>) value)) return true;
                List<NodePath> needRecover = value.Item1;
                List<NodePath> needRemove = value.Item2;
                //移除需要移除的
                foreach (NodePath path in needRemove)
                {
                    if (card.GetNodeOrNull(path) is {} node )
                    {
                        node.GetParent()?.RemoveChildSafely(node);
                        node.QueueFreeSafely();
                    }
                    else
                    {
                        HakureiReimuMain.Logger.Info("No find node:"+path);
                    }
                }
                //获取标准模板
                NCard template=NodePool.Get<NCard>();
                if (template.Body==null)
                {
                    template._Ready();
                }
                //记录唯一名称
                Dictionary<NodePath, NodePath> uniqueNames=new();
                foreach (NodePath path in needRecover)
                {
                    if (card.GetNodeOrNull(path) is {} node )
                    {
                        RecordUniqueName(card, node, uniqueNames);
                    }
                }
                //从模板恢复节点
                foreach (NodePath path in needRecover)
                {
                    Godot.Node parent = null;
                    Godot.Node sibling = null;
                    if (card.GetNodeOrNull(path) is {} old )
                    {
                        parent = old.GetParent();
                        if (parent != null)
                        {
                            int index = old.GetIndex();
                            if (index>0)
                            {
                                sibling = parent.GetChild(index - 1);
                            }
                        }
                        parent?.RemoveChildSafely(old);
                        old.QueueFreeSafelyNoPool();
                    }
                    if (parent!=null &&template.GetNodeOrNull(path) is {} newNode )
                    {
                        newNode.ReparentSafely(parent,false);
                        if (sibling != null)
                        {
                            parent.MoveChild(newNode,sibling.GetIndex()+1);
                        }
                        else
                        {
                            parent.MoveChildSafely(newNode,0);
                        }
                    }
                }
                
                template.QueueFreeSafelyNoPool();
                //重建唯一名称
                RecoveryUniqueName(card, uniqueNames);
                
                ModelField.SetValue(card,null);
                card._Ready();
                HasModify.Remove(card);
            }
            catch (Exception e)
            {
                HakureiReimuMain.Logger.Info("Recover NCard Failed"+e);
                return false;
            }
            return true;
        }
        
        // [HarmonyPatch(typeof(NCard),nameof(NCard.Model), MethodType.Setter)]
        public static class OldNCardModelSetPatch
        {
            private static readonly MethodInfo UnsubscribeFromModel = AccessTools.Method(typeof(NCard), "UnsubscribeFromModel");
            private static readonly FieldInfo CardOverlay = AccessTools.Field(typeof(NCard), "_cardOverlay");
            // [HarmonyPrefix]
            public static bool Prefix(NCard __instance,ref CardModel ____model,CardModel value)
            {
                
                if (____model!=value&&____model is INCardModify modify)
                {
                    try
                    {
                        UnsubscribeFromModel.Invoke(__instance, [____model]);
                        //获取模板
                        NCard template=NodePool.Get<NCard>();
                        if (template.Body==null)
                        {
                            template._Ready();
                        }
                        Control control = template.Body;
                        
                        //记录UniqueName
                        Dictionary<NodePath, NodePath> uniqueNames=new();
                        RecordUniqueName(__instance,__instance.Body, uniqueNames);
                        
                        //保留移动vfx
                        foreach (Godot.Node vfx in __instance.CardVfxContainer.GetChildren().ToList())
                        {
                            // RecordPosition(__instance,vfx,positions);
                            vfx.ReparentSafely(template.CardVfxContainer,false);
                        }
                        
                        //移除
                        __instance.Body.Free();
                        CardOverlay.SetValue(__instance,null);
                        
                        control.ReparentSafely(__instance,false);
                        
                        ____model = template.Model;
                        template.QueueFreeSafelyNoPool();
                        
                        //重构UniqueName
                        RecoveryUniqueName(__instance, uniqueNames);
                        
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

        private static void RecordUniqueName(Godot.Node root, Godot.Node node, Dictionary<NodePath, NodePath> uniqueNames)
        {
            if (node.UniqueNameInOwner && node.Owner != null)
            {
                uniqueNames[root.GetPathTo(node)] = root.GetPathTo(node.Owner);
            }
            foreach (Godot.Node child in node.GetChildren())
            {
                RecordUniqueName(root,child, uniqueNames);
            }
        }

        private static void RecoveryUniqueName(Godot.Node root, Dictionary<NodePath, NodePath> uniqueNames)
        {
            foreach (var keyValuePair in uniqueNames)
            {
                if (root.GetNodeOrNull(keyValuePair.Key) is {} node && root.GetNodeOrNull(keyValuePair.Value) is {} owner)
                {
                    node.Owner = owner;
                    node.UniqueNameInOwner = true;
                }
            }
        }
    }
}