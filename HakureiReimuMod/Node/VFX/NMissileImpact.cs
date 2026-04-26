using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    public partial class NMissileImpact :NVfxParticleOneShot
    {
        public static NMissileImpact Create(float scale=1)
        {
            NLargeMagicMissileVfx source = PreloadManager.Cache.GetScene(NLargeMagicMissileVfx.scenePath).Instantiate<NLargeMagicMissileVfx>();
            NMissileImpact vfx = new ();
            Node2D impact = source.GetNodeOrNull<Node2D>("impact_vfx_container");
            if (impact!=null)
            {
                impact.ReparentSafely(vfx);
                SetLocal(impact);
            }
            source.QueueFreeSafely();
            vfx.Scale = scale * Vector2.One;
            return vfx;
        }
        private static void SetLocal(Godot.Node node)
        {
            if (node is GpuParticles2D particles2D)
            {
                particles2D.LocalCoords = true;
            }
            foreach (Godot.Node child in node.GetChildren())
            {
                SetLocal(child);
            }
        }
    }
}