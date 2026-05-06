using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using HakureiReimu.HakureiReimuMod.Enchant;
using HakureiReimu.HakureiReimuMod.Patches;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
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
        public static readonly List<(int, Func<Player, Reward>,Func<Player,bool>,Action<LocString>)> RewardCreator = [
            (30,p=>new CardReward(new CardCreationOptions(GetCardPools(p,CardRarity.Common),CardCreationSource.Other,CardRarityOddsType.Uniform),3,p),null,null),
            (20,p=>new CardReward(new CardCreationOptions(GetCardPools(p,CardRarity.Uncommon),CardCreationSource.Other,CardRarityOddsType.Uniform),3,p),null,null),
            (10,p=>new CardReward(new CardCreationOptions(GetCardPools(p,CardRarity.Rare),CardCreationSource.Other,CardRarityOddsType.Uniform),3,p),null,null),
            (10,p=>new RelicReward(RelicRarity.Common,p),null,null),
            (5,p=>new RelicReward(RelicRarity.Uncommon,p),null,null),
            (2,p=>new RelicReward(RelicRarity.Rare,p),null,null),
            (30,p=>new UpgradeReward(p),p=>p.Deck.Cards.Any(c=>c.IsUpgradable),null),
            (15,p=>new TransformReward(p),p=>p.Deck.Cards.Any(c=>c.Type!=CardType.Quest&&c.IsTransformable),null),
            (5,p=>new CardRemovalReward(p),p=>p.Deck.Cards.Any(c=>c.IsRemovable),null),
            (2,p=>new CloneReward(p),p=>p.Deck.Cards.Any(),null),
            
            (10,p=>new EnchantReward(p,ModelDb.Enchantment<Sharp>().Id,p.PlayerRng.Rewards.NextInt(2,5)),p=>
            {//锋利
                EnchantmentModel e = ModelDb.Enchantment<Sharp>();
                return p.Deck.Cards.Any(e.CanEnchant);
            },s=>s.Add("Enchant",ModelDb.Enchantment<Sharp>().Title)),
            (10,p=>new EnchantReward(p,ModelDb.Enchantment<Nimble>().Id,p.PlayerRng.Rewards.NextInt(2,5)),p=>
            {//灵巧
                EnchantmentModel e = ModelDb.Enchantment<Nimble>();
                return p.Deck.Cards.Any(e.CanEnchant);
            },s=>s.Add("Enchant",ModelDb.Enchantment<Nimble>().Title)),
            (10,p=>new EnchantReward(p,ModelDb.Enchantment<Swift>().Id,p.PlayerRng.Rewards.NextInt(1,3)),p=>
            {//迅速
                EnchantmentModel e = ModelDb.Enchantment<Swift>();
                return p.Deck.Cards.Any(e.CanEnchant);
            },s=>s.Add("Enchant",ModelDb.Enchantment<Swift>().Title)),
            (5,p=>new EnchantReward(p,ModelDb.Enchantment<Vigorous>().Id,p.PlayerRng.Rewards.NextInt(5,10)),p=>
            {//活力
                EnchantmentModel e = ModelDb.Enchantment<Vigorous>();
                return p.Deck.Cards.Any(e.CanEnchant);
            },s=>s.Add("Enchant",ModelDb.Enchantment<Vigorous>().Title)),
            (5,p=>new EnchantReward(p,ModelDb.Enchantment<Steady>().Id),p=>
            {//稳定
                EnchantmentModel e = ModelDb.Enchantment<Steady>();
                return p.Deck.Cards.Any(e.CanEnchant);
            },s=>s.Add("Enchant",ModelDb.Enchantment<Steady>().Title)),
            (3,p=>new EnchantReward(p,ModelDb.Enchantment<Light>().Id,p.PlayerRng.Rewards.NextInt(1,2)),p=>
            {//轻盈
                EnchantmentModel e = ModelDb.Enchantment<Light>();
                return p.Deck.Cards.Any(e.CanEnchant);
            },s=>s.Add("Enchant",ModelDb.Enchantment<Light>().Title)),
            (2,p=>new EnchantReward(p,ModelDb.Enchantment<Glam>().Id),p=>
            {//华彩
                EnchantmentModel e = ModelDb.Enchantment<Glam>();
                return p.Deck.Cards.Any(e.CanEnchant);
            },s=>s.Add("Enchant",ModelDb.Enchantment<Glam>().Title)),
            (1,p=>new EnchantReward(p,ModelDb.Enchantment<Imbued>().Id),p=>
            {//注能
                EnchantmentModel e = ModelDb.Enchantment<Imbued>();
                return p.Deck.Cards.Any(e.CanEnchant);
            },s=>s.Add("Enchant",ModelDb.Enchantment<Imbued>().Title)),
        ];

        public Reward RollRewards(Rng rng)
        {
            List<(int, Func<Player, Reward>, Func<Player, bool>, Action<LocString>)> list = RewardCreator.Where(s=>s.Item3==null||s.Item3(Player)).ToList();
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

        public static HoverTip HoverTip
        {
            get
            {
                string id = typeof(BlindBoxReward).GetPrefix() + StringHelper.Slugify(nameof(BlindBoxReward));
                LocString name=new("gameplay_ui", id+".name");
                LocString desc=new("gameplay_ui", id+".tip");
                decimal rate=100m/RewardCreator.Select(e=>e.Item1).Sum();
                StringBuilder sb=new();
                bool line = true;
                for (var i = 0; i < RewardCreator.Count; i++)
                {
                    var value = RewardCreator[i];
                    LocString s = LocString.GetIfExists("gameplay_ui", id + ".tip." + (i + 1));
                    if (s!=null)
                    {
                        sb.Append(line ? '\n' : ' ');
                        line = !line;
                        value.Item4?.Invoke(s);
                        sb.Append(s.GetFormattedText());
                        sb.Append(':');
                        sb.Append((rate * value.Item1).ToString("F1"));
                        sb.Append('%');
                    }
                }
                desc.Add("Desc", sb.ToString());
                return new HoverTip(name, desc);
            }
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