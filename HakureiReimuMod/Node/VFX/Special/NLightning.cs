using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node.VFX.Special
{
    public partial class NLightning :Node2D
    {
        public static readonly string Path = "lightning.tscn".ScenePath();

        public static NLightning Create(float scale=1)
        {
            NLightning instantiate = PreloadManager.Cache.GetScene(Path).Instantiate<NLightning>();
            instantiate.Scale = scale * Vector2.One;
            return instantiate;
        }
    }
}