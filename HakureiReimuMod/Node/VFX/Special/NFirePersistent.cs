using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;

namespace HakureiReimu.HakureiReimuMod.Node.VFX.Special
{
    public partial class NFirePersistent : Node2D
    {
        public static readonly string Path = SceneHelper.GetScenePath("vfx/fires/vfx_stepped_fire");

        public static Node2D Create(float? scale=null,Color? color = null)
        {
            Node2D instantiate = PreloadManager.Cache.GetScene(Path).Instantiate<Node2D>();
            if (scale!=null)
            {
                instantiate.Scale = scale.Value * Vector2.One;
                GpuParticles2D particles2D = instantiate.GetNodeOrNull<GpuParticles2D>("sparks big");
                if (particles2D != null)
                {
                    particles2D.Emitting = false;
                }
            }
            if (color != null)
            {
                Sprite2D sprite2D = instantiate.GetNodeOrNull<Sprite2D>("SteppedFireMix");
                if (sprite2D != null)
                {
                    if (sprite2D.Material is ShaderMaterial material)
                    {
                        sprite2D.Material = (Material)material.Duplicate();
                        ((ShaderMaterial)sprite2D.Material).SetShaderParameter("OuterColor",color.Value);
                    }
                }
                else
                {
                    HakureiReimuMain.Logger.Warn("NFirePersistent can't find Sprite2D");
                }
            }
            return instantiate;
        }
    }
}