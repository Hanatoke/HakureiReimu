using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    public partial class NNova :NVfxParticleOneShot
    {
        public static readonly string Path = "nova.tscn".ScenePath();

        public static NNova Create(float scale=1,Color? color=null)
        {
            NNova nova = PreloadManager.Cache.GetScene(Path).Instantiate<NNova>();
            nova.Lifetime = 1;
            nova.Scale = Vector2.One * scale;
            nova.GetNode<GpuParticles2D>("Wave").Modulate = color ?? new Color(1, 1, 1, 0);
            return nova;
        }

        public override void _Ready()
        {
            base._Ready();
            GpuParticles2D node = GetNode<GpuParticles2D>("BackBufferCopy/Wave");
            ShaderMaterial material = (ShaderMaterial)node.Material.Duplicate();
            node.Material = material;
            Tween tween = CreateTween();
            tween.TweenProperty(material, "shader_parameter/amplitude", 0, 1)
                .SetTrans(Tween.TransitionType.Linear);
        }
    }
}