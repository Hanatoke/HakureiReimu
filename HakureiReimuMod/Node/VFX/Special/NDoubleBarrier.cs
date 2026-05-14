using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node.VFX.Special
{
    public partial class NDoubleBarrier :NVfxParticleOneShot
    {
        public static readonly string Path = "double_barrier.tscn".ScenePath();
        public Node2D Display;
        
        public static NDoubleBarrier Create(float scale = 1, Vector2? dir=null)
        {
            NDoubleBarrier d=PreloadManager.Cache.GetScene(Path).Instantiate<NDoubleBarrier>();
            d.Scale = scale * Vector2.One;
            (d.Display??=d.GetNode<Node2D>("Display")).Rotation=(dir ?? Vector2.Right).Angle();
            return d;
        }
    }
}