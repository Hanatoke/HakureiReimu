using System.Threading;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace HakureiReimu.HakureiReimuMod.PersistCard.Node
{
    [GlobalClass]
    public partial class NPersistCardHolder :NCardHolder
    {
        public static readonly string ScenePath = "PersistCardHolder.tscn".ScenePath();
        public TextureRect SuperFlashNode;
        public Tween SuperFlashTween;
        public Tween FlashTween;
        public Tween FlashCountTween;
        public Label Count;
        // public Godot.Node Table;
        public Marker2D NormalPos;
        public Marker2D HoverPos;
        public Control CardSet;
        public Control CardPos;
        public Control FlashNode;
        public CancellationTokenSource AngleCancelToken;
        public CancellationTokenSource PositionCancelToken;
        public CancellationTokenSource CardPositionCancelToken;
        public CancellationTokenSource ScaleCancelToken;
        private Vector2 _targetPosition;
        private float _targetAngle;
        private Vector2 _targetScale;
        public Vector2 HoverOffset =new(0,-50);
        public Vector2 HoverFontScale = Vector2.One * 2;
        public Vector2 NormalFontScale = Vector2.One * 1.2f;
        public float TargetAlpha { get; set; } = 1;
        public override Vector2 SmallScale => Vector2.One;
        public NCard HoverTip;
        public float TipScale => 1/Scale.X*1f;
        public bool UseFloatHover = true;
        public bool IsEnabled { get; protected set; } = true;

        public static NPersistCardHolder Create(NCard card)
        {
            NPersistCardHolder holder = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NPersistCardHolder>();
            holder.Name = $"{holder.GetType().Name}-{card.Model.Id}";
            holder.CardPos=holder.GetNode<Control>("%CardPos");
            holder.SetCard(card);
            return holder;
        }

        public override void _Ready()
        {
            ConnectSignals();
            CardSet=this.GetNode<Control>("%CardSet");
            CardPos=this.GetNode<Control>("%CardPos");
            FlashNode=this.GetNode<Control>("%Flash");
            NormalPos = this.GetNode<Marker2D>("%NormalPos");
            HoverPos = this.GetNode<Marker2D>("%HoverPos");
            SuperFlashNode=this.GetNode<TextureRect>("%SuperFlash");
            this.SuperFlashNode.Modulate = new Color(this.SuperFlashNode.Modulate.R, this.SuperFlashNode.Modulate.G, this.SuperFlashNode.Modulate.B, 0.0f);
            Count = this.GetNode<Label>("%Count");
            // CardSet.Scale = 1.25f * Vector2.One;
            Modulate=new Color(1f, 1f, 1f, TargetAlpha);
            UpdateCard();
            Hitbox.SetEnabled(false);
        }
        public override void _ExitTree()
        {
            base._ExitTree();
            this.UnsubscribeFromEvents(this.CardNode?.Model);
            this.StopAnimations();
        }

        public override void Clear()
        {
            this.UnsubscribeFromEvents(this.CardNode?.Model);
            base.Clear();
            this.StopAnimations();
        }

        public virtual void SetEnabled(bool enabled)
        {
            IsEnabled=enabled;
        }
        protected override void OnFocus()
        {
            this.EmitSignal(NHandCardHolder.SignalName.HolderFocused, (Variant) (GodotObject) this);
            base.OnFocus();
        }

        protected override void OnUnfocus()
        {
            this.EmitSignal(NHandCardHolder.SignalName.HolderUnfocused, (Variant) (GodotObject) this);
            base.OnUnfocus();
        }

        protected override void OnMousePressed(InputEvent inputEvent)
        {
            
        }

        protected override void OnMouseReleased(InputEvent inputEvent)
        {
            
        }

        protected override void DoCardHoverEffects(bool isHovered)
        {
            if (!IsEnabled)
            {
                isHovered=false;
            }
            this.ZIndex = isHovered ? 1 : 0;
            if (isHovered)
            {
                if (UseFloatHover)
                {
                    ShowCardTip();
                    OnHover();
                }
                else
                {
                    this.CreateHoverTips();
                }
            }
            else
            {
                HideCardTip();
                this.ClearHoverTips();
                OnUnhover();
            }
            
        }

        protected override void CreateHoverTips()
        {
            base.CreateHoverTips();
        }

        public void Flash()
        {
            Flash(NCardFlash.FlashColor);
        }
        public void Flash(Color c)
        {
            NCardFlash f = NCardFlash.Create(c);
            CardPos.AddChildSafely(f);
            CardPos.MoveChildSafely(f,0);
            // f.Scale = 0.8f * Vector2.One;

            CardPos.Scale = Vector2.One * 1.5f;
            FlashTween?.Kill();
            FlashTween = CreateTween();
            FlashTween.TweenProperty(CardPos, "scale", Vector2.One, 0.25).SetEase(Tween.EaseType.Out);
        }

        public void FlashCount(Color c)
        {
            Count.Scale=8f * Vector2.One;
            Count.Modulate = c;
            FlashCountTween?.Kill();
            FlashCountTween = CreateTween();
            FlashCountTween.TweenProperty(Count, "scale", NormalFontScale, 0.25).SetEase(Tween.EaseType.Out);
            FlashCountTween.Parallel().TweenProperty(Count, "modulate", new Color(1, 1, 1), 0.25).SetEase(Tween.EaseType.Out);
        }
        

        public void ShowCardTip()
        {
            HoverTip?.QueueFreeSafely();
            if (CardNode?.Model!=null)
            {
                if (!IsInstanceValid(CardNode)||!CardNode.IsInsideTree())return;
                HoverTip = NCard.Create(CardNode.Model);
                if (HoverTip!=null)
                {
                    this.AddChildSafely(HoverTip);
                    HoverTip.SetForceUnpoweredPreview(false);
                    HoverTip.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
                    HoverTip.Scale = Vector2.One * TipScale;
                    HoverTip.Position=new Vector2(0,NPersistCardTable.CardHeight*(TipScale+1)/2);
                    NHoverTipSet tipSet=NHoverTipSet.CreateAndShow(HoverTip, this.CardNode.Model.HoverTips);
                    tipSet.SetAlignment(HoverTip,MegaCrit.Sts2.Core.HoverTips.HoverTip.GetHoverTipAlignment(this));
                    tipSet.SetFollowOwner();
                    tipSet.SetExtraFollowOffset(new Vector2(NPersistCardTable.CardWidth*TipScale/2,-NPersistCardTable.CardHeight*TipScale/2)*this.Scale);
                }
            }
        }

        public void HideCardTip()
        {
            HoverTip?.QueueFreeSafely();
            HoverTip = null;
        }

        public virtual void OnHover()
        {
            Modulate=new Color(1, 1, 1,1);
            SetCardTargetPosition(HoverOffset);
            Count.Scale = HoverFontScale;
            Count.GlobalPosition = HoverPos.GlobalPosition;
        }

        public virtual void OnUnhover()
        {
            Modulate=new Color(1, 1, 1,TargetAlpha);
            SetCardTargetPosition(Vector2.Zero);
            Count.Scale = NormalFontScale;
            Count.GlobalPosition = NormalPos.GlobalPosition;
        }
        public void StopAnimations()
        {
            DoCardHoverEffects(false);
            this.AngleCancelToken?.Cancel();
            this.PositionCancelToken?.Cancel();
            this.ScaleCancelToken?.Cancel();
        }

        public void SetCount(int count,bool visible)
        {
            Count.SetText(count.ToString());
            Count.Visible = visible;
        }
        protected override void SetCard(NCard node)
        {
            if (this.CardNode != null)
                this.CardNode.ModelChanged -= (this.OnModelChanged);
            this.UnsubscribeFromEvents(this.CardNode?.Model);
            this.CardNode = node;
            if (this.CardNode.GetParent() == null)
            {
                CardPos.AddChildSafely( CardNode);
            }
            else
            {
                CardNode.ReparentSafely(CardPos);
            }
            this.UpdateCard();
            this.SubscribeToEvents(this.CardNode?.Model);
            if (this.CardNode != null)
                this.CardNode.ModelChanged += (this.OnModelChanged);
        }

        public void UpdateCard()
        {
            if (!this.IsNodeReady() || this.CardNode == null)
                return;
            CardNode.SetForceUnpoweredPreview(false);
            this.CardNode.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
            CardNode.CardHighlight.Modulate = NCardHighlight.playableColor;
            CardNode.CardHighlight.AnimHide();
        }
        public void SuperFlash()
        {
            
            if (!IsInstanceValid(this.SuperFlashNode))
                return;
            this.SuperFlashNode.Scale = Vector2.One;
            this.SuperFlashNode.Modulate = NCardHighlight.playableColor;
            this.SuperFlashTween?.Kill();
            this.SuperFlashTween = this.CreateTween();
            this.SuperFlashTween.TweenProperty( this.SuperFlashNode, (NodePath) "modulate:a", (Variant) 0.6, 0.15);
            this.SuperFlashTween.TweenProperty( this.SuperFlashNode, (NodePath) "modulate:a", (Variant) 0, 0.3);
        }
        public void OnModelChanged(CardModel oldModel)
        {
            this.UnsubscribeFromEvents(oldModel);
            this.SubscribeToEvents(this.CardNode?.Model);
        }
        
        private void SubscribeToEvents(CardModel card)
        {
            if (card == null || !this.IsInsideTree())
                return;
            card.Upgraded +=(this.SuperFlash);
            card.KeywordsChanged += (this.SuperFlash);
            card.ReplayCountChanged +=(this.SuperFlash);
            card.AfflictionChanged += (this.SuperFlash);
            card.EnergyCostChanged += (this.SuperFlash);
            card.StarCostChanged += (this.SuperFlash);
        }

        private void UnsubscribeFromEvents(CardModel card)
        {
            if (card == null)
                return;
            card.Upgraded -=SuperFlash;
            card.KeywordsChanged -= SuperFlash;
            card.ReplayCountChanged -= SuperFlash;
            card.AfflictionChanged -=SuperFlash;
            card.EnergyCostChanged -= SuperFlash;
            card.StarCostChanged -= SuperFlash;
        }
        public void SetTargetAngle(float angle,bool instant=false)
        {
            this._targetAngle = angle;
            this.AngleCancelToken?.Cancel();
            if (instant)
            {
                this.RotationDegrees = angle;
                return;
            }
            this.AngleCancelToken = new CancellationTokenSource();
            TaskHelper.RunSafely(this.AnimAngle(this.AngleCancelToken));
        }

        public void SetTargetPosition(Vector2 position,bool instant=false)
        {
            this._targetPosition = position;
            this.PositionCancelToken?.Cancel();
            if (instant)
            {
                this.Position = position;
                return;
            }
            this.PositionCancelToken = new CancellationTokenSource();
            TaskHelper.RunSafely(this.AnimPosition(this.PositionCancelToken));
        }

        public void SetCardTargetPosition(Vector2 position,bool instant=false)
        {
            this.CardPositionCancelToken?.Cancel();
            if (instant)
            {
                CardSet.Position = position;
                return;
            }
            this.CardPositionCancelToken = new CancellationTokenSource();
            TaskHelper.RunSafely(this.AnimCardPosition(this.CardPositionCancelToken,position));
        }

        public void SetTargetScale(Vector2 scale,bool instant=false)
        {
            this._targetScale = scale;
            this.ScaleCancelToken?.Cancel();
            if (instant)
            {
                this.Scale = scale;
                return;
            }
            this.ScaleCancelToken = new CancellationTokenSource();
            TaskHelper.RunSafely(this.AnimScale(this.ScaleCancelToken));
        }
        
        
        private async Task AnimAngle(CancellationTokenSource cancelToken)
      {
        NPersistCardHolder holder = this;
        while (!cancelToken.IsCancellationRequested)
        {
          holder.RotationDegrees = Mathf.Lerp(holder.RotationDegrees, holder._targetAngle, (float) holder.GetProcessDeltaTime() * 10f);
          if ((double) Mathf.Abs(holder.RotationDegrees - holder._targetAngle) < 0.10000000149011612)
          {
            holder.RotationDegrees = holder._targetAngle;
            break;
          }
          Variant[] signal = await holder.ToSignal((GodotObject) holder.GetTree(), SceneTree.SignalName.ProcessFrame);
        }
      }

      private async Task AnimScale(CancellationTokenSource cancelToken)
      {
          NPersistCardHolder nhandCardHolder = this;
        while (!cancelToken.IsCancellationRequested)
        {
          nhandCardHolder.Scale = nhandCardHolder.Scale.Lerp(nhandCardHolder._targetScale, (float) nhandCardHolder.GetProcessDeltaTime() * 8f);
          if ((double) Mathf.Abs(nhandCardHolder._targetScale.X - nhandCardHolder.Scale.X) < 1.0 / 500.0)
          {
            nhandCardHolder.Scale = nhandCardHolder._targetScale;
            break;
          }
          Variant[] signal = await nhandCardHolder.ToSignal((GodotObject) nhandCardHolder.GetTree(), SceneTree.SignalName.ProcessFrame);
        }
      }

      private async Task AnimPosition(CancellationTokenSource cancelToken)
      {
          NPersistCardHolder nhandCardHolder = this;
        while (!cancelToken.IsCancellationRequested)
        {
          nhandCardHolder.Position = nhandCardHolder.Position.Lerp(nhandCardHolder._targetPosition, (float) nhandCardHolder.GetProcessDeltaTime() * 7f);
          float num = Mathf.Abs(nhandCardHolder.Position.X - nhandCardHolder._targetPosition.X);
          if (!nhandCardHolder.Hitbox.IsEnabled && (double) num < 200.0)
            nhandCardHolder.Hitbox.SetEnabled(true);
          if ((double) nhandCardHolder.Position.DistanceSquaredTo(nhandCardHolder._targetPosition) < 1.0)
          {
            nhandCardHolder.Position = nhandCardHolder._targetPosition;
            return;
          }
          Variant[] signal = await nhandCardHolder.ToSignal((GodotObject) nhandCardHolder.GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        if (nhandCardHolder.Hitbox.IsEnabled || (double) nhandCardHolder.Position.DistanceSquaredTo(nhandCardHolder._targetPosition) >= 200.0)
          return;
        nhandCardHolder.Hitbox.SetEnabled(true);
      }
      private async Task AnimCardPosition(CancellationTokenSource cancelToken,Vector2 position)
      {
          Control set = CardSet;
          while (!cancelToken.IsCancellationRequested)
          {
              set.Position = set.Position.Lerp(position, (float) set.GetProcessDeltaTime() * 7f);
              if (set.Position.DistanceSquaredTo(position) < 1.0)
              {
                  set.Position = position;
                  return;
              }
              await set.ToSignal((GodotObject) set.GetTree(), SceneTree.SignalName.ProcessFrame);
          }
      }
    }
}