using System;
using System.Collections.Generic;
using Godot;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HakureiReimu.HakureiReimuMod.Node
{
    [GlobalClass]
    public partial class NYinYangOrbManager :Control
    {
        protected NCreature CreatureNode;
        protected float TweenSpeed = 0.45f;
        protected Tween Tween;
        public bool IsLocal { get; protected set; }
        public List<NYinYangOrb> Orbs { get; protected set; } = new();
        public Creature PreviewTarget { get; protected set; }

        protected Player Player => this.CreatureNode.Entity.Player;
        protected readonly static Random Random = new();

        public static NYinYangOrbManager Create(NCreature creature,bool isLocal)
        {
            if (creature.Entity.Player == null)
                throw new InvalidOperationException("NYinYangOrbManager can only be applied to player creatures");
            NYinYangOrbManager manager = new();
            manager.CreatureNode = creature;
            manager.IsLocal = isLocal;
            return manager;
        }

        public override void _Ready()
        {
            Vector2 offset = CreatureNode.Visuals.OrbPosition.Position;
            this.Position = offset/2;
            // if (!IsLocal)
            // {
            //     Scale *= 0.85f;
            // }
        }

        public override void _EnterTree()
        {
            base._EnterTree();
            CombatManager.Instance.StateTracker.CombatStateChanged += this.OnCombatStateChanged;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            CombatManager.Instance.StateTracker.CombatStateChanged -= this.OnCombatStateChanged;
        }

        public virtual void OnCardPlayHover(Creature target)
        {
            PreviewTarget = target;
            UpdateVisuals(PreviewTarget);
        }
        
        protected virtual void OnCombatStateChanged(CombatState _){
            UpdateVisuals(PreviewTarget);
        }
        
        public virtual void UpdateVisuals(Creature target = null)
        {
            foreach (NYinYangOrb nOrb in Orbs)
            {
                nOrb.UpdateVisuals(target);
            }
        }

        public virtual void AddOrb(IEnumerable<YinYangOrb> orbs)
        {
            foreach (YinYangOrb o in orbs)
            {
                NYinYangOrb nOrb = NYinYangOrb.Create(LocalContext.IsMe(Player),o);
                this.AddChildSafely(nOrb);
                Orbs.Add(nOrb);
                nOrb.Position = Vector2.Zero;
            }
            TweenLayout();
            UpdateControllerNavigation();
        }

        public virtual NYinYangOrb PopOrb(out Vector2 position)
        {
            if (Orbs.Count>0)
            {
                int index = Random.Next(0, Orbs.Count);
                NYinYangOrb nOrb = Orbs[index];
                position = nOrb.GlobalPosition;
                this.RemoveChildSafely(nOrb);
                Orbs.RemoveAt(index);
                TweenLayout();
                UpdateControllerNavigation();
                return nOrb;
            }
            position=this.GlobalPosition;
            return null;
        }

        public virtual void TweenLayout()
        {
            this.Tween?.Kill();
            if (Orbs.Count<=0) return;
            this.Tween = this.CreateTween().SetParallel();
            float dist = 160 + Orbs.Count * 10;
            if (!IsLocal)
            {
                dist *= 0.75f;
            }
            float angle = 100 + Orbs.Count * 12;
            float offset = angle / 2;
            for (var i = 0; i < Orbs.Count; i++)
            {
                float x, y;
                if (Orbs.Count==1)
                {
                    x = 0;
                    y = dist;
                }
                else
                {
                    float a=angle*((float)i/(Orbs.Count - 1));
                    a +=90-offset;
                    a = float.DegreesToRadians(a);
                    x=dist*Mathf.Cos(a);
                    y=dist*Mathf.Sin(a);
                }
                Tween.TweenProperty(Orbs[i],"position",new Vector2(x,-y), TweenSpeed).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
            }
        }
        private void UpdateControllerNavigation()
        {
            for (int index = 0; index < this.Orbs.Count; ++index)
            {
                NYinYangOrb orb = this.Orbs[index];
                NodePath path;
                if (index <= 0)
                {
                    List<NYinYangOrb> orbs = this.Orbs;
                    path = orbs[orbs.Count - 1].GetPath();
                }
                else
                    path = this.Orbs[index - 1].GetPath();
                orb.FocusNeighborRight = path;
                this.Orbs[index].FocusNeighborLeft = index < this.Orbs.Count - 1 ? this.Orbs[index + 1].GetPath() : this.Orbs[0].GetPath();
                this.Orbs[index].FocusNeighborTop = this.Orbs[index].GetPath();
                this.Orbs[index].FocusNeighborBottom = this.CreatureNode.Hitbox.GetPath();
            }
            this.CreatureNode.UpdateNavigation();
        }
    }
}