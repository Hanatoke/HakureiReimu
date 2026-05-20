using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    public partial class NShatter :NVfxParticleOneShot
    {
        public static readonly string Path = "shatter.tscn".ScenePath();
        public Texture2D Texture;
        public GpuParticles2D Main;
        public static readonly Vector2 BaseSize = new (256, 256);

        public static NShatter Create(Texture2D texture, float scale = 1)
        {
            NShatter shatter = PreloadManager.Cache.GetScene(Path).Instantiate<NShatter>();
            shatter.Texture = texture;
            shatter.Scale = scale * Vector2.One;
            return shatter;
        }

        public override void _Ready()
        {
            Main = GetNode<GpuParticles2D>("Main");
            Main.Texture = this.Texture;
            if (Texture != null)
            {
                Main.Scale = BaseSize / Texture.GetSize();
            }
            base._Ready();
        }
    }
}