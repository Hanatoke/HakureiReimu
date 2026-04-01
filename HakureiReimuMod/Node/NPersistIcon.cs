using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node
{
    [GlobalClass]
    public partial class NPersistIcon :Control
    {
        public static readonly string Path = "persist_icon.tscn".ScenePath();
        public bool NeedHover = false;
        public Tween FadeTween;
        public float FadeTime = 0.25f;
        public bool IsHover { get;protected set; }
        public TextureRect Img;
        public static Control Create(bool needHover=false)
        {
            NPersistIcon icon = PreloadManager.Cache.GetScene(Path).Instantiate<NPersistIcon>();
            icon.NeedHover = needHover;
            return icon;
        }
        public override void _Ready()
        {
            Img = GetNode<TextureRect>("Img");
            if (NeedHover)
            {
                Modulate = new Color(1, 1, 1, 0);
            }
        }
        // public override void _Process(double delta)
        // {
        //     if (IsNodeReady()&&NeedHover)
        //     {
        //         Vector2 mousePosition = GetViewport().GetMousePosition();
        //         if (GetGlobalRect().HasPoint(mousePosition))
        //         {
        //             OnHover();
        //         }
        //         else
        //         {
        //             OnUnhover();
        //         }
        //     }
        // }
        protected virtual void OnHover()
        {
            if (!IsHover&&IsNodeReady()&&NeedHover)
            {
                IsHover=true;
                FadeTween?.Kill();
                FadeTween = CreateTween();
                FadeTween.TweenProperty(this, "modulate:a", 1, FadeTime);
            }
        }

        protected virtual void OnUnhover()
        {
            if (IsHover&&IsNodeReady()&&NeedHover)
            {
                IsHover=false;
                FadeTween?.Kill();
                FadeTween = CreateTween();
                FadeTween.TweenProperty(this, "modulate:a", 0, FadeTime);
            }
        }
    }
}