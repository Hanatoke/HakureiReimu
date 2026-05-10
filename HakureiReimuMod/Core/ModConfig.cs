using BaseLib.Config;

namespace HakureiReimu.HakureiReimuMod.Core
{
    [ConfigHoverTipsByDefault]
    public class ModConfig :SimpleModConfig
    {
        [ConfigSlider(0,100)]
        public static int MaxFollowDanmakuCount
        {
            get => FollowDanmakuManager.MaxFollows;
            set => FollowDanmakuManager.MaxFollows = value;
        }
        public static bool UseStaticEnergyIcon { get; set; } = false;
        public static bool UseStaticEnergyCounter { get; set; } = false;
    }
}