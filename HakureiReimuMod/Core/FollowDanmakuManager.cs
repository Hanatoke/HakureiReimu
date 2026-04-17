using System.Collections.Generic;
using System.Linq;
using Godot;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public static class FollowDanmakuManager
    {
        public static readonly Dictionary<CardModel,List<Node2D>> DanmakuNodes = new();

        public static List<Node2D> GetFollows(this CardModel card)
        {
            if (!DanmakuNodes.TryGetValue(card, out var list))
            {
                list = [];
                DanmakuNodes[card] = list;
            }
            return list;
        }

        public static void AddFollow(this CardModel card, Node2D node,float? a=null, float? b=null,float? revolutionSpeed=null,float? orbitalRotationSpeed=null)
        {
            if (NCombatRoom.Instance==null||CombatManager.Instance.IsOverOrEnding)return;
            NCombatRoom room = NCombatRoom.Instance;
            NCreature owner = room.GetCreatureNode(card.Owner.Creature);
            if (!GodotObject.IsInstanceValid(owner))return;
            FollowVFX vfx = FollowVFX.Create(() => GodotObject.IsInstanceValid(owner) ? owner.VfxSpawnPosition : null,
                a, b, revolutionSpeed, orbitalRotationSpeed);
            vfx.AddChildSafely(node);
            if (!LocalContext.IsMe(card.Owner)) vfx.Modulate = new Color(Colors.White, 0.5f);
            room.BackCombatVfxContainer.AddChildSafely(vfx);
            GetFollows(card).Add(vfx);
        }

        public static bool TryGetFollow<T>(this CardModel card, out T node) where T : Node2D
        {
            List<Node2D> follows = card.GetFollows();
            for (int i = 0; i < follows.Count; i++)
            {
                Node2D follow = follows[i];
                T n = follow.GetChildren().OfType<T>().FirstOrDefault();
                if (n != null)
                {
                    follows.RemoveAt(i);
                    node = n;
                    return true;
                }
            }

            node = null;
            return false;
        }

        public static void ClearFollows(this CardModel card)
        {
            List<Node2D> follows = card.GetFollows();
            foreach (Node2D node2D in follows)
            {
                node2D.QueueFreeSafely();
            }
            follows.Clear();
        }

        public static void Clear()
        {
            foreach (var keyValuePair in DanmakuNodes)
            {
                keyValuePair.Key.ClearFollows();
            }
            DanmakuNodes.Clear();
            MainFile.Logger.Info(nameof(FollowDanmakuManager)+" cleared");
        }
    }
}