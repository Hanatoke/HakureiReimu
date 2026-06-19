using BaseLib.Config;
using HakureiReimu.HakureiReimuMod.Cards;

namespace HakureiReimu.HakureiReimuMod.Core
{
    [ConfigHoverTipsByDefault]
    public class ModConfig :SimpleModConfig
    {
        [ConfigSlider(0,1,0.01,Format = "{0:P0}")]
        public static float CounterCardFlashDelayScale
        {
            get=>AbstractPersistCard.FlashDelayScale;
            set=>AbstractPersistCard.FlashDelayScale=value;
        }
        [ConfigSlider(0,100)]
        public static int MaxFollowDanmakuCount
        {
            get => FollowDanmakuManager.MaxFollows;
            set => FollowDanmakuManager.MaxFollows = value;
        }
        [ConfigSlider(0.5,3,0.01,Format = "{0:P0}")]
        public static double FlyingVfxAnimationSpeed { get; set; } = 1;
        public static bool UseStaticEnergyIcon { get; set; } = false;
        public static bool UseStaticEnergyCounter { get; set; } = false;
        public static bool ShowSealTotal { get; set; } = true;
    }
}