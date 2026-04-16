using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node.VFX
{
    [GlobalClass]
    public partial class NYinYangOrbFlying :Node2D
    {
        public static readonly string Path = "yin_yang_orb_flying.tscn".ScenePath();

        public static NYinYangOrbFlying Create(float scale=1)
        {
            NYinYangOrbFlying orb=PreloadManager.Cache.GetScene(Path).Instantiate<NYinYangOrbFlying>();
            orb.Scale = scale*Vector2.One;
            return orb;
        }
    }
}