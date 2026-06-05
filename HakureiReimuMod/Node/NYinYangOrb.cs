using System.Collections.Generic;
using Godot;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
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
        public Control _visualContainer;
        public Control _labelContainer;
        public MegaLabel _passiveLabel;
        public MegaLabel _evokeLabel;
        public Control _bounds;
        public CpuParticles2D _flashParticle;
        public NSelectionReticle _selectionReticle;
        public bool _isLocal;
        public Node2D _sprite;
        public Tween _curTween;
        // private Font defaultFont;
        // public Font GetDefaultFont
        // {
        //     get
        //     {
        //         if (this.defaultFont == null)
        //         {
        //             NOrb nOrb = NOrb.Create(true);
        //             var label = nOrb.GetNode<MegaLabel>("%PassiveAmount");
        //             defaultFont = label.GetThemeFont(ThemeConstants.Label.Font);
        //             nOrb.QueueFreeSafely();
        //         }
        //         return defaultFont;
        //     }
        // }

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
            this.UpdateVisuals(false);
            // _passiveLabel.AddThemeFontOverride(ThemeConstants.Label.Font,defaultFont);
            // _evokeLabel.AddThemeFontOverride(ThemeConstants.Label.Font, defaultFont);
        }

        public override void _EnterTree()
        {
            base._EnterTree();
            if (this.Model == null)
                return;
            this.Model.Triggered += this.Flash;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            if (this.Model == null)
                return;
            this.Model.Triggered -= this.Flash;
        }

        public void ReplaceOrb(YinYangOrb model)
        {
            Node2D sprite = this._sprite;
            if (sprite != null)
                sprite.QueueFreeSafely();
            this._sprite = (Node2D)null;
            this.Model = model;
            this.UpdateVisuals(false);
        }

        public void UpdateVisuals(bool isEvoking)
        {
            if (!this.IsNodeReady() || !CombatManager.Instance.IsInProgress)
                return;
            if (this.Model == null)
            {
                Node2D sprite = this._sprite;
                if (sprite != null)
                    sprite.QueueFreeSafely();
                this._passiveLabel.Visible = false;
                this._evokeLabel.Visible = false;
                this._outline.Visible = this._isLocal;
                this._flashParticle.Visible = false;
            }
            else
            {
                if (this._sprite == null)
                {
                    this._sprite = this.Model.CreateSprite();
                    this._visualContainer.AddChildSafely(this._sprite);
                    this._sprite.Position = Vector2.Zero;
                    this._curTween?.Kill();
                    this._curTween = this.CreateTween();
                    this._curTween
                        .TweenProperty(this._sprite, "scale", Vector2.One, 0.5)
                        .From(Vector2.Zero).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
                }

                this._outline.Visible = false;
                this._flashParticle.Visible = true;
                this._flashParticle.Texture = this.Model.Icon;
                this._labelContainer.Visible = this._isLocal;
                if (!this._isLocal)
                    this.Modulate = this.Model.DarkenedColor;
                switch (this.Model)
                {
                    default:
                        this._passiveLabel.Visible = !isEvoking;
                        this._evokeLabel.Visible = isEvoking;
                        this._passiveLabel.SetTextAutoSize(this.Model.PassiveVal.ToString("0"));
                        this._evokeLabel.SetTextAutoSize(this.Model.EvokeVal.ToString("0"));
                        break;
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
            if (!NControllerManager.Instance.IsUsingController)
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