using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    public partial class NDanmakuImpact:NVfxParticleSystem
    {
        public static readonly string Path = "danmaku_impact.tscn".ScenePath();
        public static NDanmakuImpact Create(float scale=1,Color? color=null)
        {
            NDanmakuImpact impact=PreloadManager.Cache.GetScene(Path).Instantiate<NDanmakuImpact>();
            impact.Scale=Vector2.One * scale;
            impact.GetNode<Node2D>("ColorAble").Modulate = color??Color.FromHsv(GD.Randf(), 1, 1);
            return impact;
        }

        protected void StartVfx(Godot.Node node)
        {
            if (node is GpuParticles2D particles2D)
            {
                particles2D.Emitting = true;
            }
            foreach (Godot.Node child in node.GetChildren())
            {
                StartVfx(child);
            }
        }

        public override void _Ready()
        {
            StartVfx(this);
            base._Ready();
        }
    }
}