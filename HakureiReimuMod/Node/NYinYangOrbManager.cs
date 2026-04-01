using System;
using System.Collections.Generic;
using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Patches;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Orbs;

namespace HakureiReimu.HakureiReimuMod.Node
{
    [GlobalClass]
    public partial class NYinYangOrbManager :Control
    {
        protected NCreature CreatureNode;
        protected float MinRadius = 225f;
        protected float MaxRadius = 300f;
        protected float Range = 150f;
        protected float AngleOffset = -25f;
        protected float TweenSpeed = 0.45f;
        protected Tween Tween;
        public bool IsLocal { get; protected set; }
        public List<NOrb> Orbs { get; protected set; } = new();

        protected Player Player => this.CreatureNode.Entity.Player;

        public static NYinYangOrbManager Create(NCreature creature,bool isLocal)
        {
            if (creature.Entity.Player == null)
                throw new InvalidOperationException("NYinYangOrbManager can only be applied to player creatures");
            NYinYangOrbManager manager = new();
            manager.CreatureNode = creature;
            manager.IsLocal = isLocal;
            return manager;
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
        
        protected virtual void OnCombatStateChanged(CombatState _){
            UpdateVisuals();
        }
        
        public virtual void UpdateVisuals()
        {
            
        }

        public virtual void TweenLayout()
        {
            if (Player.PlayerCombatState==null)return;
            int capacity = Player.PlayerCombatState.YinYangOrbManager()?.Capacity ?? 0;
            if (capacity == 0)
                return;
            float num1 = 125f;
            float num2 = num1 / (capacity - 1);
            float num3 = Mathf.Lerp(MinRadius, MaxRadius, (float) ((capacity - 3.0) / 7.0));
            if (!this.IsLocal)
                num3 *= 0.75f;
            this.Tween?.Kill();
            this.Tween = this.CreateTween().SetParallel();
            for (int index = 0; index < capacity; ++index)
            {
                float radians = float.DegreesToRadians(AngleOffset - num1);
                Vector2 finalVal = new Vector2(-Mathf.Cos(radians), Mathf.Sin(radians)) * num3;
                this.Tween.TweenProperty(this.Orbs[index], "position", finalVal, TweenSpeed).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
                num1 -= num2;
            }
        }
        private void UpdateControllerNavigation()
        {
            for (int index = 0; index < this.Orbs.Count; ++index)
            {
                NOrb orb = this.Orbs[index];
                NodePath path;
                if (index <= 0)
                {
                    List<NOrb> orbs = this.Orbs;
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