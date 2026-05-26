using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node.VFX.Special
{
    public partial class NPhantom :Node2D
    {
        public static readonly string Path = "phantom.tscn".ScenePath();
        public Sprite2D Main;
        public GpuParticles2D Wave;

        public static NPhantom Create(int? count = null)
        {
            NPhantom phantom = PreloadManager.Cache.GetScene(Path).Instantiate<NPhantom>();
            if (count!=null)phantom.SetCount(count.Value);
            return phantom;
        }
        public override void _Ready()
        {
            Main = GetNode<Sprite2D>("%Main");
            Wave = GetNode<GpuParticles2D>("%Wave");
        }

        public void SetCount(int count)
        {
            if (Main==null)
            {
                _Ready();
            }
            if (Main?.Material is ShaderMaterial shader)
            {
                shader.SetShaderParameter("ghost_count", count);
            }
        }
    }
}