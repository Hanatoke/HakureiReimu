using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;

namespace HakureiReimu.HakureiReimuMod.Node.VFX.Special
{
    public partial class NRain : Node2D
    {
        public static readonly string Path = "rain.tscn".ScenePath();

        public static NRain Create(float scale = 1f)
        {
            NRain rain = PreloadManager.Cache.GetScene(Path).Instantiate<NRain>();
            rain.Scale = scale * Vector2.One;
            return rain;
        }
    }
}