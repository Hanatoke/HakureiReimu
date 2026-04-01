using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.PersistCard.Node
{
    [GlobalClass]
    public partial class NCardFlash :Control
    {
        public static readonly Color FlashColor = new Color(0.2f, 0.9f, 1f);
        public TextureRect Img1 { get; set; }
        public TextureRect Img2 { get; set; }
        public TextureRect Img3 { get; set; }
        public Texture2D Image;
        public Color Color;
        public float StartDuration = 0.5f;
        public float Duration { get; set; }
        public static readonly string ScenePath = "CardFlash.tscn".ScenePath();

        public static NCardFlash Create(Color color)
        {
            NCardFlash flash=PreloadManager.Cache.GetScene(ScenePath).Instantiate<NCardFlash>();
            flash.Color=color;
            return flash;
        }
        public override void _Ready()
        {
            Duration = StartDuration;
            Img1=this.GetNode<TextureRect>("Img1");
            Img2=this.GetNode<TextureRect>("Img2");
            Img3=this.GetNode<TextureRect>("Img3");
            if (Image!=null)
            {
                Img1.Texture = Image;
                Img2.Texture = Image;
                Img3.Texture = Image;
            }
        }

        public override void _Process(double delta)
        {
            Duration -= (float)delta;
            if (Duration <= 0.0f)
            {
                this.QueueFree();
            }
            Color.A = Duration;
            Img1.Modulate = Color;
            Img2.Modulate = Color;
            Img3.Modulate = Color;
            float scale = Mathf.Lerp(1.1f, 1.2f, (StartDuration-Duration)/StartDuration);
            Img1.Scale = Vector2.One * (scale + 1) * 0.52f;
            Img2.Scale = Vector2.One * (scale + 1) * 0.55f;
            Img3.Scale = Vector2.One * (scale + 1) * 0.58f;
        }
    }
}