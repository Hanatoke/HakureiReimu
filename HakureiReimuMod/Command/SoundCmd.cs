using Godot;

namespace HakureiReimu.HakureiReimuMod.Command
{
    public static class SoundCmd
    {
        public static readonly string[] DanmakuImpact = [
            "res://HakureiReimu/audio/danmaku/Impact_1.wav",
            "res://HakureiReimu/audio/danmaku/Impact_2.wav",
            "res://HakureiReimu/audio/danmaku/Impact_3.wav",
        ];
        public static readonly string[] DanmakuLaunch = [
            "res://HakureiReimu/audio/danmaku/Launch_1.wav",
            "res://HakureiReimu/audio/danmaku/Launch_2.wav",
            "res://HakureiReimu/audio/danmaku/Launch_3.wav",
        ];

        public static void PlayFile(string path,float volume=1)
        {
            //TODO:等待新版本Baselib
        }

        public static void PlayDanmakuImpact(float volume = 1)
        {
            PlayFile(DanmakuImpact[GD.RandRange(0, DanmakuImpact.Length-1)], volume);
        }

        public static void PlayDanmakuLaunch(float volume = 1)
        {
            PlayFile(DanmakuLaunch[GD.RandRange(0, DanmakuLaunch.Length-1)], volume);
        }
    }
}