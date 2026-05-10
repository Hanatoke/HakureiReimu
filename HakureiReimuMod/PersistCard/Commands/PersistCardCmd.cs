using System;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.PersistCard.Extensions;
using HakureiReimu.HakureiReimuMod.PersistCard.Node;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

namespace HakureiReimu.HakureiReimuMod.PersistCard.Commands
{
    public static class PersistCardCmd
    {
        public static async Task StartPersistCard(AbstractPersistCardTable table,AbstractPersistCardSlot slot)
        {
            if (table==null)
            {
                HakureiReimuMain.Logger.Warn("PersistCardCmd.StartPersistCard: table is null");
                return;
            }
            if (slot?.Card?.CombatState == null)
            {
                HakureiReimuMain.Logger.Warn("PersistCardCmd.StartPersistCard: slot.Card.combatState is null");
                HakureiReimuMain.Logger.Warn("slot:"+slot);
                return;
            }
            if (CombatManager.Instance.IsOverOrEnding)return;
            if (slot.Card.Pile!=table)
            {
                CardModel card = slot.Card;
                NCard nCard=NCard.FindOnTable(card, card.Pile?.Type);
                card.RemoveFromCurrentPile();
                table.AddInternal(card);
                table.AddSlot(card,slot);
                if (nCard==null)
                {
                    nCard=NCard.Create(card);
                    if (NCombatRoom.Instance is {} room)
                    {
                        room.Ui.AddChildSafely(nCard);
                        nCard.Position = room.Ui.GetViewportRect().GetCenter();
                    }
                }
                else
                {
                    nCard.PlayPileTween?.Kill();
                    // nCard.Scale = Vector2.One;
                    nCard.Modulate=Colors.White;
                }

                NPersistCardTable nt = NCombatRoom.Instance?.GetCreatureNode(card.Owner.Creature)
                    ?.PersistCardTable(table);
                if (nt!=null&&GodotObject.IsInstanceValid(nt))
                {
                    NPersistCardHolder holder=nt.AddCard(nCard);
                    holder.SetCount(slot.Count,slot.ShouldDisplayCount);
                }
                await PersistCardHook.OnStartPersistCard(card.CombatState, slot);
            }
        }

        public static async Task StopPersistCard(AbstractPersistCardSlot slot,PileType? overridePile=null,bool skipVisuals=false)
        {
            if (slot?.Card?.CombatState == null)
            {
                HakureiReimuMain.Logger.Warn("PersistCardCmd.StopPersistCard: slot.Card.combatState is null");
                HakureiReimuMain.Logger.Warn("slot:"+slot);
                return;
            }
            if (CombatManager.Instance.IsOverOrEnding)return;
            if (slot.Card.Pile is AbstractPersistCardTable)
            {
                CardModel card = slot.Card;
                ICombatState state = card.CombatState;
                PileType targetPile = overridePile ?? PileType.Discard;
                if (!overridePile.HasValue&&slot.Card.IsDupe)
                {
                    if (!skipVisuals)
                    {
                        StopPersistCardVisual(slot, PileType.None);
                    }
                    await CardPileCmd.RemoveFromCombat(slot.Card,skipVisuals:true);
                }
                else if (!overridePile.HasValue&&(slot.Card.ExhaustOnNextPlay||slot.Card.Keywords.Contains(CardKeyword.Exhaust)))
                {
                    if (!skipVisuals)
                    {
                        StopPersistCardVisual(slot, PileType.Exhaust);
                    }
                    await CardCmd.Exhaust(new BlockingPlayerChoiceContext(),slot.Card,skipVisuals:true);
                    card.Pile?.InvokeCardAddFinished();
                }
                else
                {
                    if (!skipVisuals)
                    {
                        StopPersistCardVisual(slot, targetPile);
                    }
                    await CardPileCmd.Add(slot.Card, targetPile,skipVisuals:true);
                }
                await PersistCardHook.OnStopPersistCard(state, slot);
            }
        }

        private static MethodInfo _appendPlayPileLerpTween;
        private static MethodInfo AppendPlayPileLerpTween => _appendPlayPileLerpTween ??=
            AccessTools.Method(typeof(CardPileCmd), "AppendPlayPileLerpTween",
                [typeof(Tween), typeof(NCard), typeof(CardPile)]);
        private static MethodInfo _appendPileLerpTween;
        private static MethodInfo AppendPileLerpTween => _appendPileLerpTween ??= AccessTools.Method(
            typeof(CardPileCmd), "AppendPileLerpTween", [
                typeof(Tween),
                typeof(NCard),
                typeof(PileType),
                typeof(CardPile)
            ]);

        public static void StopPersistCardVisual(AbstractPersistCardSlot slot, PileType targetPile)
        {
            if (slot?.Card == null)
            {
                HakureiReimuMain.Logger.Warn("PersistCardCmd.StopPersistCardVisual: slot.Card is null");
                HakureiReimuMain.Logger.Warn("slot:"+slot);
                return;
            }
            if (CombatManager.Instance.IsOverOrEnding)return;
            if (slot.Card.Pile is AbstractPersistCardTable table)
            {
                CardModel card = slot.Card;
                bool isLocal = LocalContext.IsMe(card.Owner);
                //获取table
                NPersistCardTable nTable = NCombatRoom.Instance?.GetCreatureNode(slot.Card.Owner.Creature)
                    ?.PersistCardTable(table);
                //获取holder
                NPersistCardHolder holder = null;
                if (nTable!=null&&GodotObject.IsInstanceValid(nTable))
                {
                    holder = nTable.GetCardHolder(slot.Card);
                    holder?.SetEnabled(false);
                }
                //获取NCard
                NCard nCard = holder?.CardNode;
                switch (targetPile)
                {
                    case PileType.None:
                        TweenExhaustCard(holder,nTable);
                        break;
                    case PileType.Exhaust:
                        card.Pile?.InvokeCardAddFinished();
                        TweenExhaustCard(holder,nTable);
                        break;
                    case PileType.Play:
                        if (nCard!=null)
                        {
                            nCard.ReparentSafely(NCombatRoom.Instance.Ui);
                            Tween tween = NCombatRoom.Instance.Ui.CreateTween().SetParallel();
                            AppendPlayPileLerpTween.Invoke(null, [tween, nCard, null]);
                        }
                        if (holder!=null)
                        {
                            nTable?.RemoveCardHolder(holder);
                        }
                        break;
                    case PileType.Hand:
                        if (nCard != null&&isLocal)
                        {
                            nCard.ReparentSafely(NCombatRoom.Instance.Ui);
                            Tween tween = NCombatRoom.Instance.Ui.CreateTween().SetParallel();
                            AppendPileLerpTween.Invoke(null, [tween, nCard, targetPile, null]);
                            tween.Parallel().TweenCallback(Callable.From(() => NCombatRoom.Instance.Ui.Hand.Add(nCard)));
                        }
                        if (holder!=null)
                        {
                            nTable?.RemoveCardHolder(holder);
                        }
                        break;
                    default:
                        if (nCard != null&&isLocal)
                        {
                            nCard.ReparentSafely(NCombatRoom.Instance.Ui);
                            Tween tween = NCombatRoom.Instance.Ui.CreateTween().SetParallel();
                            tween.TweenCallback(Callable.From((Action) (() =>
                            {
                                Godot.Node node = NCombatRoom.Instance?.CombatVfxContainer ;
                                if (node==null)
                                {
                                    nCard.QueueFreeSafely();
                                    return;
                                };
                                nCard.ReparentSafely(node);
                                NCardFlyVfx child = NCardFlyVfx.Create(nCard, card.Pile.Type.GetTargetPosition(nCard), true, card.Owner.Character.TrailPath);
                                node.AddChildSafely( child);
                            })));
                        }
                        if (holder!=null)
                        {
                            nTable?.RemoveCardHolder(holder);
                        }
                        break;
                }
            }
        }

        public static async Task IncreaseCount(AbstractPersistCardSlot slot,int amount)
        {
            if (slot?.Card?.CombatState == null)
            {
                HakureiReimuMain.Logger.Warn("PersistCardCmd.IncreaseCount: slot.Card.combatState is null");
                HakureiReimuMain.Logger.Warn("slot:"+slot);
                return;
            }
            if (CombatManager.Instance.IsOverOrEnding)return;
            int origin = slot.Count;
            if (PersistCardHook.AtIncreaseCount(slot.Card.CombatState,slot,origin,ref amount))
            {
                slot.Count = origin + amount;
                await PersistCardHook.AfterModifyPersistCount(slot.Card.CombatState, slot,slot.Count);
            }
        }

        public static async Task DecreaseCount(AbstractPersistCardSlot slot,int amount)
        {
            if (slot?.Card?.CombatState == null)
            {
                HakureiReimuMain.Logger.Warn("PersistCardCmd.DecreaseCount: slot.Card.combatState is null");
                HakureiReimuMain.Logger.Warn("slot:"+slot);
                return;
            }
            if (CombatManager.Instance.IsOverOrEnding)return;
            int origin = slot.Count;
            if (PersistCardHook.AtDecreaseCount(slot.Card.CombatState,slot,origin,ref amount))
            {
                slot.Count = origin - amount;
                await PersistCardHook.AfterModifyPersistCount(slot.Card.CombatState, slot,slot.Count);
            }
        }

        public static async Task FlashPersistCard(AbstractPersistCardSlot slot,float duration=0.8f)
        {
            if (slot?.Card?.CombatState == null)
            {
                HakureiReimuMain.Logger.Warn("PersistCardCmd.AbstractPersistCardSlot: slot.Card.combatState is null");
                HakureiReimuMain.Logger.Warn("slot:"+slot);
                return;
            }
            if (CombatManager.Instance.IsOverOrEnding)return;
            if (slot.Card.Pile is AbstractPersistCardTable table)
            {
                NPersistCardTable nt = NCombatRoom.Instance?.GetCreatureNode(slot.Card.Owner.Creature)
                    ?.PersistCardTable(table);
                if (nt != null&&GodotObject.IsInstanceValid(nt))
                {
                    NPersistCardHolder holder=nt.GetCardHolder(slot.Card);
                    if (holder==null||!GodotObject.IsInstanceValid(holder))return;
                    holder.StopAnimations();
                    Vector2 size = NCombatRoom.Instance.Ui.GetViewportRect().Size;
                    Vector2 global = NCombatRoom.Instance.Ui.GetGlobalTransformWithCanvas() * new Vector2(size.X*0.5f,size.Y*0.4f);
                    Vector2 local = nt.GetGlobalTransformWithCanvas().AffineInverse() * global;
                    holder.Modulate = Colors.White;
                    holder.SetTargetPosition(local);
                    holder.Flash();
                    holder.ZIndex = 10;
                    slot.PlaySfx(true);
                    await Cmd.Wait(duration);
                    holder.Modulate = new Color(1, 1, 1, holder.TargetAlpha);
                    holder.ZIndex = 0;
                    nt.RefreshLayout();
                }
            }
        }

        public static void TweenExhaustCard(NPersistCardHolder holder,NPersistCardTable table)
        {
            if (holder!=null&&NCombatRoom.Instance!=null)
            {
                table.ReparentCardHolder(holder,NCombatRoom.Instance.Ui);
                NCard nc = holder.CardNode;
                if (nc != null)
                {
                    holder.SetTargetPosition(holder.Position+new Vector2(0,-200));
                    Tween tween = NCombatRoom.Instance.Ui.CreateTween().SetParallel();
                    tween.Chain().TweenCallback(Callable.From((Action) (() => NCombatRoom.Instance.Ui.AddChildSafely(NExhaustVfx.Create(nc)))));
                    tween.Parallel().TweenProperty( nc, "modulate", (Variant) StsColors.exhaustGray,  0.2);
                    tween.Chain().TweenCallback(Callable.From((holder.QueueFreeSafely)));
                }
                else
                {
                    holder.QueueFreeSafely();
                }
            }
        }
    }
}