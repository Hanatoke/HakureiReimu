using BaseLib.Config;

namespace HakureiReimu.HakureiReimuMod.Core
{
    [HoverTipsByDefault]
    public class ModConfig :SimpleModConfig
    {
        [SliderRange(0,100)]
        public static int MaxFollowDanmakuCount
        {
            get => FollowDanmakuManager.MaxFollows;
            set => FollowDanmakuManager.MaxFollows = value;
        }
        public static bool UseStaticEnergyIcon { get; set; } = false;
        public static bool UseStaticEnergyCounter { get; set; } = false;
    }
}