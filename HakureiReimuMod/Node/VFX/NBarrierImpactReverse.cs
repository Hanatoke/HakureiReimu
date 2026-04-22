using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    public partial class NBarrierImpactReverse :NVfxParticleOneShot
    {
        public static readonly string Path = "barrier_impact_reverse.tscn".ScenePath();

        public static NBarrierImpactReverse Create(float scale = 1f,float duration=1)
        {
            NBarrierImpactReverse impact = PreloadManager.Cache.GetScene(Path).Instantiate<NBarrierImpactReverse>();
            impact.Scale=Vector2.One * scale;
            impact.Lifetime=duration;
            return impact;
        }
    }
}