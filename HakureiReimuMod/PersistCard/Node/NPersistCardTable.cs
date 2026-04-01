using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.PersistCard.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace HakureiReimu.HakureiReimuMod.PersistCard.Node
{
    [GlobalClass]
    public partial class NPersistCardTable :Control
    {
        public const float CardWidth = 300f;
        public const float CardHeight = 422f;
        public float Gap  = 5f;
        public float Width  = 700;
        public Vector2 Offset => new(-Width/2,-575);
        public float CardScale { get; set; } = 0.45f;
        public float CardAlpha { get; set; } = 1f;
        public NCreature CreatureNode {get;protected set;}
        // public AbstractPersistCardTable Table {get;protected set;}
        public List<NPersistCardHolder> CardHolders { get; private set; } = [];
        public bool IsLocal {get;private set;}
        protected Vector2[] CardPositions { get; private set; } = [];
        public virtual PileType PileType { get; protected set; }= PileType.None;

        public bool Folding { get; protected set; } = false;
        private Player Player => CreatureNode.Entity.Player;
        // protected Tween Tween;

        public static NPersistCardTable Create(NCreature creature,AbstractPersistCardTable t,bool IsLocal)
        {
            if (creature?.Entity==null)
            {
                throw new InvalidOperationException("NPersistCardTable can only be applied to creature");
            }
            NPersistCardTable table = new();
            table.CreatureNode=creature;
            table.IsLocal=IsLocal;
            table.PileType= t.Type;
            if (!IsLocal)
            {
                table.SetNoLocal();
            }
            return table;
        }

        public void SetNoLocal()
        {
            Width *= 0.5f;
            // Scale *= 0.5f;
            CardScale *= 0.5f;
            // Modulate = new Color(Modulate.R,Modulate.G,Modulate.B,Modulate.A*0.5f);
            CardAlpha = 0.5f;
        }
        
        public override void _EnterTree()
        {
            base._EnterTree();
            CombatManager.Instance.StateTracker.CombatStateChanged += this.OnCombatStateChanged;
            CombatManager.Instance.CombatSetUp += this.OnCombatSetup;
            Position = Offset;
            if (!IsLocal)
            {
                Position = new Vector2(Position.X, Position.Y * 0.7f);
            }
        }
        public override void _ExitTree()
        {
            base._ExitTree();
            CombatManager.Instance.StateTracker.CombatStateChanged -= this.OnCombatStateChanged;
            CombatManager.Instance.CombatSetUp -= this.OnCombatSetup;
        }
        private void OnCombatSetup(CombatState state)
        {
            
        }

        private void OnCombatStateChanged(CombatState state)
        {
            UpdateVisuals();
        }

        public virtual void TweenLayout()
        {
            AbstractPersistCardTable table = Player.PlayerCombatState.PersistCardTable(PileType);
            if (table == null)return;
            CalculatePosition(CardScale);
            // float alpha = IsLocal ? 1 : 0.75f;
            for (var i = 0; i < CardHolders.Count; i++)
            {
                var holder = CardHolders[i];
                holder.SetTargetPosition(CardPositions[i]);
                holder.SetTargetScale(CardScale*Vector2.One);
                holder.SetTargetAngle(0);
            }
        }

        protected virtual void CalculatePosition(float scale)
        {
            Folding=false;
            if (CardHolders.Count<=0)return;
            CardPositions = new Vector2[CardHolders.Count];
            float cw = CardWidth*scale;
            float ch = CardHeight*scale;
            float availableWidth = Width-((Width+Gap)%(cw+Gap));
            List<Vector2> temp = new();
            if (CardHolders.Count*cw + Gap*(CardHolders.Count-1)<=availableWidth)
            {
                int i = 0;
                foreach (var c in CardHolders)
                {
                    temp.Add(new Vector2(cw*i+cw/2+Gap*i,ch/2));
                    i++;
                }
            }
            else
            {
                float len = availableWidth - cw;
                for (int i = 0; i < CardHolders.Count; i++)
                {
                    float n=(float)i/(CardHolders.Count-1);
                    temp.Add(new Vector2(len*n+cw/2,ch/2));
                }
                Folding=true;
            }
            CardPositions = temp.ToArray();
        }

        protected virtual void UpdateVisuals()
        {
            foreach (var c in CardHolders)
            {
                c.UpdateCard();
            }
        }

        protected virtual void UpdateControllerNavigation()
        {
            for (var i = 0; i < CardHolders.Count; i++)
            {
                CardHolders[i].FocusNeighborTop = CardHolders[i].GetPath();
                CardHolders[i].FocusNeighborBottom = CreatureNode.Hitbox.GetPath();
                CardHolders[i].FocusNeighborLeft = i-1<0?CardHolders[^1].GetPath():CardHolders[i-1].GetPath();
                CardHolders[i].FocusNeighborRight=i+1>=CardHolders.Count?CardHolders[0].GetPath():CardHolders[i+1].GetPath();
            }
            CreatureNode.UpdateNavigation();
        }
        //----------------------------------------------------
        public virtual NPersistCardHolder AddCard(NCard card)
        {
            return InsertCard(card,CardHolders.Count);
        }

        public virtual void RemoveCard(NCard card)
        {
            NPersistCardHolder holder = GetCardHolder(card);
            if (holder!=null)
            {
                RemoveCardHolder(holder);
            }
            RefreshLayout();
        }

        public virtual NPersistCardHolder GetCardHolder(NCard card)
        {
            return CardHolders.FirstOrDefault(h=>h.CardNode==card);
        }

        public virtual NPersistCardHolder GetCardHolder(CardModel card)
        {
            foreach (NPersistCardHolder h in CardHolders)
            {
                if (h.CardNode?.Model==card)
                {
                    return h;
                }
            }

            return null;
        }
        

        public virtual NPersistCardHolder InsertCard(NCard card, int index)
        {
            Vector2 globalPosition=card.GlobalPosition;
            Vector2 scale=card.Scale;
            NPersistCardHolder holder = NPersistCardHolder.Create(card);
            holder.TargetAlpha = CardAlpha;
            AddCardHolder(holder, index);
            holder.GlobalPosition = globalPosition;
            holder.Scale = scale;
            card.Scale = Vector2.One;
            RefreshLayout();
            return  holder;
        }

        public virtual void RefreshLayout()
        {
            TweenLayout();
            UpdateControllerNavigation();
        }
        public virtual void AddCardHolder(NPersistCardHolder holder, int index)
        {
            this.AddChildSafely( holder);
            this.MoveChildSafely(holder, index);
            CardHolders.Insert(index,holder);
            this.RefreshLayout();
            if (!this.HasFocus())
                return;
            holder.TryGrabFocus();
        }

        public virtual void Clear()
        {
            foreach (Godot.Node child in GetChildren())
            {
                child.QueueFreeSafely();
            }
            CardHolders.Clear();
        }

        public virtual void RemoveCardHolder(NPersistCardHolder holder)
        {
            bool flag = holder.HasFocus();
            holder.Clear();
            holder.GetParent().RemoveChildSafely( holder);
            holder.QueueFreeSafely();
            CardHolders.Remove(holder);
            this.RefreshLayout();
            if (!flag)
                return;
            this.TryGrabFocus();
        }

        public virtual void ReparentCardHolder(NPersistCardHolder holder,Godot.Node newParent)
        {
            bool flag = holder.HasFocus();
            holder.ReparentSafely(newParent);
            CardHolders.Remove(holder);
            this.RefreshLayout();
            if (!flag)
                return;
            this.TryGrabFocus();
        }
    }
}