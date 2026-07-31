using System;
using System.Collections.Generic;
using Godot;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Node.VFX.Special;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace HakureiReimu.HakureiReimuMod.Node
{
    public partial class NYinYangOrb : NClickableControl
    {
        public static readonly string Path = "nyin_yang_orb.tscn".ScenePath();
        public TextureRect _outline;
        public Control BackVfxContainer;
        public Control FrontVfxContainer;
        public Control _visualContainer;
        public Control _labelContainer;
        public MegaLabel _passiveLabel;
        public MegaLabel _evokeLabel;
        public Control _bounds;
        public CpuParticles2D _flashParticle;
        public NSelectionReticle _selectionReticle;
        public bool _isLocal;
        public Node2D Sprite;
        public Tween _curTween;
        public NLightning lightning;
        public Node2D FirePersistent;
        public NRain Rain;

        public YinYangOrb Model { get; private set; }
        

        public static NYinYangOrb Create(bool isLocal)
        {
            NYinYangOrb norb = PreloadManager.Cache.GetScene(Path).Instantiate<NYinYangOrb>();
            norb._isLocal = isLocal;
            return norb;
        }
        public static NYinYangOrb Create(bool isLocal, YinYangOrb? model)
        {
            NYinYangOrb norb = Create(isLocal);
            norb.Model = model;
            return norb;
        }

        public override void _Ready()
        {
            this.ConnectSignals();
            this._outline = this.GetNode<TextureRect>("%Outline");
            BackVfxContainer=this.GetNode<Control>("%BackVfxContainer");
            FrontVfxContainer=this.GetNode<Control>("%FrontVfxContainer");
            this._visualContainer = this.GetNode<Control>("%VisualContainer");
            this._passiveLabel = this.GetNode<MegaLabel>("%PassiveAmount");
            this._evokeLabel = this.GetNode<MegaLabel>("%EvokeAmount");
            this._flashParticle = this.GetNode<CpuParticles2D>("%Flash");
            this._bounds = this.GetNode<Control>("Bounds");
            this._labelContainer = this.GetNode<Control>("%LabelContainer");
            this._selectionReticle = this.GetNode<NSelectionReticle>("%SelectionReticle");
            if (this.Model != null)
                this.CreateTween().TweenProperty(this._outline, "scale", Vector2.One, 0.25).From(Vector2.Zero);
            this.Scale *= 0.85f;
            this.UpdateVisuals();
            // _passiveLabel.AddThemeFontOverride(ThemeConstants.Label.Font,defaultFont);
            // _evokeLabel.AddThemeFontOverride(ThemeConstants.Label.Font, defaultFont);
        }

        // public override void _EnterTree()
        // {
        //     base._EnterTree();
        //     if (this.Model == null)
        //         return;
        //     this.Model.Triggered += this.Flash;
        // }
        //
        // public override void _ExitTree()
        // {
        //     base._ExitTree();
        //     if (this.Model == null)
        //         return;
        //     this.Model.Triggered -= this.Flash;
        // }

        public void ReplaceOrb(YinYangOrb model)
        {
            Node2D sprite = this.Sprite;
            if (sprite != null)
                sprite.QueueFreeSafely();
            this.Sprite = (Node2D)null;
            this.Model = model;
            this.UpdateVisuals();
        }

        public void UpdateVisuals(Creature target=null)
        {
            if (!this.IsNodeReady() || !CombatManager.Instance.IsInProgress)
                return;
            if (this.Model == null)
            {
                Node2D sprite = this.Sprite;
                if (sprite != null)
                    sprite.QueueFreeSafely();
                this._passiveLabel.Visible = false;
                this._evokeLabel.Visible = false;
                this._outline.Visible = this._isLocal;
                this._flashParticle.Visible = false;
            }
            else
            {
                if (this.Sprite == null)
                {
                    this.Sprite = this.Model.CreateSprite();
                    this._visualContainer.AddChildSafely(this.Sprite);
                    this.Sprite.Position = Vector2.Zero;
                    this._curTween?.Kill();
                    this._curTween = this.CreateTween();
                    this._curTween
                        .TweenProperty(this.Sprite, "scale", Vector2.One, 0.5)
                        .From(Vector2.Zero).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
                }

                this._outline.Visible = false;
                this._flashParticle.Visible = true;
                this._flashParticle.Texture = this.Model.Icon;
                this._labelContainer.Visible = this._isLocal;
                if (!this._isLocal)
                    this.Modulate = this.Model.DarkenedColor;
                bool preview = Model.IsAvailableTarget(target);
                this._passiveLabel.Visible = !preview;
                this._evokeLabel.Visible = preview;
                this._passiveLabel.SetTextAutoSize(this.Model.PassiveVal.ToString("0"));
                decimal evokeValue = Model.EvokeVal;
                if (preview)
                    evokeValue = Hook.ModifyDamage(Model.CombatState.RunState, Model.CombatState, target, Model.Owner.Creature,
                        evokeValue, Model.DamageProp, null, null,ModifyDamageHookType.All, CardPreviewMode.Normal, out _);
                evokeValue=Math.Floor(evokeValue);
                this._evokeLabel.SetTextAutoSize(evokeValue.ToString("0"));

                if ((lightning != null) != Model.ShowLightning)
                {
                    if (lightning==null)
                    {
                        lightning = NLightning.Create();
                        FrontVfxContainer.AddChildSafely(lightning);
                    }
                    else
                    {
                        lightning.QueueFreeSafely();
                        lightning = null;
                    }
                }

                if ((FirePersistent != null) != Model.ShowFire) 
                {
                    if (FirePersistent==null)
                    {
                        FirePersistent = NFirePersistent.Create(0.2f,new Color("#bf44ff"));
                        BackVfxContainer.AddChildSafely(FirePersistent);
                        FirePersistent.Position = new Vector2(0, 20f);
                    }
                    else
                    {
                        FirePersistent.QueueFreeSafely();
                        FirePersistent = null;
                    }
                }
                if ((Rain != null) != Model.ShowRain) 
                {
                    if (Rain==null)
                    {
                        Rain = NRain.Create();
                        BackVfxContainer.AddChildSafely(Rain);
                    }
                    else
                    {
                        Rain.QueueFreeSafely();
                        Rain = null;
                    }
                }
            }
        }

        private void Flash() => this._flashParticle.Emitting = true;

        protected override void OnFocus()
        {
            if (this.Model == null && !this._isLocal)
                return;
            IEnumerable<IHoverTip> hoverTips = this.Model != null ? this.Model.HoverTips : [OrbModel.EmptySlotHoverTipHoverTip];
            NHoverTipSet.CreateAndShow(this._bounds, hoverTips, HoverTip.GetHoverTipAlignment(this._bounds))
                ?.SetFollowOwner();
            this._labelContainer.Visible = true;
            this.Modulate = Colors.White;
            if (!NControllerManager.Instance.IsUsingDirectionalNavigation)
                return;
            this._selectionReticle.OnSelect();
        }

        protected override void OnUnfocus()
        {
            this._labelContainer.Visible = this._isLocal;
            if (this.Model != null)
                this.Modulate = this._isLocal ? Colors.White : this.Model.DarkenedColor;
            NHoverTipSet.Remove(this._bounds);
            this._selectionReticle.OnDeselect();
        }
    }
}