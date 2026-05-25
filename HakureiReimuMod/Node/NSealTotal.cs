using System;
using System.Linq;
using System.Text;
using Godot;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace HakureiReimu.HakureiReimuMod.Node
{
    public partial class NSealTotal :Control
    {
        public static readonly string Path = "seal_total.tscn".ScenePath();

        public Control Visual;
        public Label Value;
        private LocString _tipName;
        private LocString _tipDescription;
        public LocString TipName => _tipName ??= new LocString("powers",ModelDb.Power<SealPower>().Id.Entry+ ".totalName");
        public LocString TipDescription => _tipDescription ??= new LocString("powers",ModelDb.Power<SealPower>().Id.Entry+ ".totalDescription");
        public Tween Tween;
        public int TotalSeal;
        public int TotalDamage;
        public Player Player;
        public bool Enabled => ModConfig.ShowSealTotal && LocalContext.IsMe(Player);

        public static NSealTotal Create(Player player)
        {
            NSealTotal nSealTotal = PreloadManager.Cache.GetScene(Path).Instantiate<NSealTotal>();
            nSealTotal.Player = player;
            return nSealTotal;
        }
        public override void _Ready()
        {
            Visual = GetNode<Control>("%Visual");
            Value = GetNode<Label>("%Value");
            this.Connect(Control.SignalName.MouseEntered, Callable.From(OnHover));
            this.Connect(Control.SignalName.MouseExited, Callable.From(OnUnhover));
            Scale = Vector2.One * 0.8f;
            Visible = false;
        }

        public override void _EnterTree() => SubscribeToCombatState();
        public override void _ExitTree() => UnSubscribeToCombatState();

        public void SubscribeToCombatState()
        {
            CombatManager.Instance.StateTracker.CombatStateChanged += UpdateVisual;
        }

        public void UnSubscribeToCombatState()
        {
            CombatManager.Instance.StateTracker.CombatStateChanged -= UpdateVisual;
        }

        public void UpdateVisual(CombatState state)
        {
            if (!Enabled)return;
            if (state==null)return;
            int newValue = state.HittableEnemies.Select(e => e.GetPowerAmount<SealPower>()).Sum();
            bool hasChanged = newValue != TotalSeal;
            TotalSeal = newValue;
            if (Player != null)
            {
                TotalDamage = 0;
                foreach (Creature enemy in state.HittableEnemies)
                {
                    if (enemy.IsMonster && enemy.Monster is { } m)
                    {
                        TotalDamage += Math.Min(enemy.GetPowerAmount<SealPower>(),
                            m.NextMove.Intents.OfType<AttackIntent>()
                                .Select(a => a.GetTotalDamage([Player.Creature], enemy)).Sum());
                    }
                }
            }
            UpdateValue();
            if (hasChanged) Flash();
            Visible = ShouldDisplay;
        }

        public bool ShouldDisplay => TotalSeal > 0;

        public void UpdateValue()
        {
            StringBuilder sb = new();
            if (TotalSeal>0)sb.Append(TotalSeal);
            if (TotalDamage>0)sb.Append($"-{TotalDamage}");
            Value.Text = sb.ToString();
            Value.Scale = Vector2.One * (TotalDamage > 0 ? 1 : 1.25f);
        }

        public void OnHover()
        {
            if (!Enabled)return;
            ShowHoverTips();
            CombatManager.Instance.StateTracker.CombatStateChanged += ShowHoverTips;
        }

        public void OnUnhover()
        {
            if (!Enabled)return;
            HideHoverTips();
            CombatManager.Instance.StateTracker.CombatStateChanged -= ShowHoverTips;
        }

        public void ShowHoverTips(CombatState _=null)
        {
            HideHoverTips();
            TipDescription.Add("TotalSeal",TotalSeal);
            TipDescription.Add("TotalDamage",TotalDamage);
            NHoverTipSet.CreateAndShow(this,
                [new HoverTip(TipName, TipDescription), HoverTipFactory.FromPower<SealPower>()],HoverTipAlignment.Right);
        }

        public void HideHoverTips()
        {
            NHoverTipSet.Remove(this);
        }
        public void Flash()
        {
            Visual.Scale = Vector2.One * 1.1f;
            Tween?.Kill();
            Tween = CreateTween();
            Tween.TweenProperty(Visual, "scale", Vector2.One, 0.5f).SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Sine);
        }
    }
}