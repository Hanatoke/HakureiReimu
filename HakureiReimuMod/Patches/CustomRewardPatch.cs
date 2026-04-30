using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace HakureiReimu.HakureiReimuMod.Patches
{
    public static class CustomRewardPatch
    {
        public static int GenerateEnum(string s)
        {
            var used = new HashSet<int>(
                Enum.GetValues<RewardType>().Cast<int>()
            );
            unchecked
            {
                int hash = Fnv1A(s) & 0x7FFFFFFF;
                hash = 800000000 + (hash % 100000000);
                while (used.Contains(hash)) hash++;
                return hash;
            }
        }
        private static int Fnv1A(string s)
        {
            unchecked
            {
                int hash = (int)2166136261;
                foreach (char c in s)
                {
                    hash ^= c;
                    hash *= 16777619;
                }
                return hash;
            }
        }
        public delegate Reward CustomRewardCreator(SerializableReward reward, Player player);
        public static Dictionary<RewardType, CustomRewardCreator> CustomRewards { get; }=new ();

        [HarmonyPatch(typeof(Reward), nameof(Reward.FromSerializable))]
        static class RewardFromSerializablePatch
        {
            [HarmonyPrefix]
            public static bool Prefix(SerializableReward save, Player player,ref Reward __result)
            {
                if (CustomRewards.TryGetValue(save.RewardType, out CustomRewardCreator creator))
                {
                    __result = creator(save, player);
                    return false;
                }
                return true;
            }
        }
    }
}