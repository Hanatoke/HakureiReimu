using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    [GlobalClass]
    public partial class NTalisman :Node2D
    {
        public static string Path = "talisman.tscn".ScenePath();
        public Node2D Visual;
        public Node2D Trails;
        public Trail TrailOuter;
        public Trail TrailInner;

        public static NTalisman Create(float scale=1,int trailLength=10)
        {
            NTalisman vfx=PreloadManager.Cache.GetScene(Path).Instantiate<NTalisman>();
            vfx.Scale = scale * Vector2.One;
            vfx.SetTrailLength(trailLength);
            return vfx;
        }

        public override void _Ready()
        {
            Visual = GetNode<Node2D>("Visual");
            Trails = Visual.GetNode<Node2D>("Trails");
            TrailOuter = Trails.GetNode<Trail>("TrailOuter");
            TrailInner = Trails.GetNode<Trail>("TrailInner");
        }
        public void SetTrailLength(int length)
        {
            if (TrailInner == null || TrailOuter == null)
            {
                _Ready();
            }
            TrailInner.MaxSegments=length;
            TrailOuter.MaxSegments=length;
        }
    }
}