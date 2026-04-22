using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    public partial class NDanmakuImpact:NVfxParticleOneShot
    {
        public static readonly string Path = "danmaku_impact.tscn".ScenePath();
        public static NDanmakuImpact Create(float scale=1,Color? color=null)
        {
            NDanmakuImpact impact=PreloadManager.Cache.GetScene(Path).Instantiate<NDanmakuImpact>();
            impact.Scale=Vector2.One * scale;
            impact.GetNode<Node2D>("ColorAble").Modulate = color??Color.FromHsv(GD.Randf(), 1, 1);
            return impact;
        }
    }
}