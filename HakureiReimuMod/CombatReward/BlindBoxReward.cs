using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using HakureiReimu.HakureiReimuMod.Patches;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace HakureiReimu.HakureiReimuMod.CombatReward
{
    public class BlindBoxReward(Player player) :AbstractReward(player)
    {
        public static readonly RewardType Type = (RewardType)CustomRewardPatch.GenerateEnum(typeof(BlindBoxReward).FullName);
        protected override RewardType RewardType=>Type;
        public override int RewardsSetIndex => 0;
        
        public override bool IsPopulated => Rewards!=null;
        
        public override Task Populate()
        {
            Rewards = [];
            for (var i = 0; i < 1; i++)
            {
                Reward reward = RollRewards(Player.PlayerRng.Rewards);
                if (reward != null)Rewards.Add(reward);
            }
            return Task.CompletedTask;
        }
        public override void MarkContentAsSeen(){}
        public List<Reward> Rewards = null;

        protected override async Task<bool> OnSelect()
        {
            if (Rewards==null || Rewards.Count == 0)return true;
            Rewards.ForEach(r=>Parent[r]=this);
            await RewardsCmd.OfferCustom(Player, Rewards);
            return Rewards.Count<=0;
        }

        public static List<CardModel> GetCardPools(Player player, CardRarity rarity)
        {
            return player.Character.CardPool
                .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
                .Where(c => c.Rarity == rarity).ToList();
        }
        public static readonly List<(int, Func<Player, Reward>,Func<Player,bool>)> RewardCreator = [
            (30,p=>new CardReward(new CardCreationOptions(GetCardPools(p,CardRarity.Common),CardCreationSource.Other,CardRarityOddsType.Uniform),3,p),null),
            (20,p=>new CardReward(new CardCreationOptions(GetCardPools(p,CardRarity.Uncommon),CardCreationSource.Other,CardRarityOddsType.Uniform),3,p),null),
            (10,p=>new CardReward(new CardCreationOptions(GetCardPools(p,CardRarity.Rare),CardCreationSource.Other,CardRarityOddsType.Uniform),3,p),null),
            (10,p=>new RelicReward(RelicRarity.Common,p),null),
            (5,p=>new RelicReward(RelicRarity.Uncommon,p),null),
            (2,p=>new RelicReward(RelicRarity.Rare,p),null),
            (30,p=>new UpgradeReward(p),p=>p.Deck.Cards.Any(c=>c.IsUpgradable)),
            (15,p=>new TransformReward(p),p=>p.Deck.Cards.Any(c=>c.IsRemovable)),
            (5,p=>new CardRemovalReward(p),p=>p.Deck.Cards.Any(c=>c.IsRemovable)),
            (2,p=>new CloneReward(p),p=>p.Deck.Cards.Any()),
        ];

        public Reward RollRewards(Rng rng)
        {
            List<(int, Func<Player, Reward>, Func<Player, bool>)> list = RewardCreator.Where(s=>s.Item3==null||s.Item3(Player)).ToList();
            int total = list.Select(e=>e.Item1).Sum();
            int random=rng.NextInt(0, total);
            int n = 0;
            foreach (var r in list)
            {
                n += r.Item1;
                if (random <= n)
                {
                    return r.Item2(Player);
                }
            }
            return null;
        }

        
        public static SpireField<Reward, BlindBoxReward> Parent = new(_=>null);
        [HarmonyPatch(typeof(Reward), nameof(OnSelectWrapper))]
        public static class RewardsPatch
        {
            [HarmonyPostfix]
            public static async Task<bool> Postfix(Task<bool> __result,Reward __instance)
            {
                bool result=await __result;
                if (result&&Parent[__instance]!=null)
                {
                    Parent[__instance].Rewards.Remove(__instance);
                }
                return result;
            }
        }
    }
}