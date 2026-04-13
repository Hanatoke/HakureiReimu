using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace HakureiReimu.HakureiReimuMod.Relics
{
    public class SaisenBox:AbstractRelic
    {
        public override RelicRarity Rarity => RelicRarity.Uncommon;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            
        ];
        public override bool ShowCounter => true;
        public override int DisplayAmount => Gold;
        private int _gold;
        [SavedProperty]
        public int Gold
        {
            get=>_gold;
            set
            {
                _gold=value;
                InvokeDisplayAmountChanged();
            }
        }
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
                new GoldVar(5),
                new DynamicVar("Heal", 1),
            ];
        public async Task LoseGold(Player player,decimal amount,GoldLossType goldLossType)
        {
            if (player!=Owner)return;
            if (amount+Gold>=DynamicVars.Gold.IntValue)
            {
                int n=Mathf.FloorToInt((float)(amount+Gold)/DynamicVars.Gold.IntValue);
                Gold +=(int) (amount + Gold) % DynamicVars.Gold.IntValue;
                Flash();
                await CreatureCmd.Heal(player.Creature, n);
            }
            else
            {
                Gold +=(int) amount;
            }
        }
        [HarmonyPatch(typeof(PlayerCmd),nameof(PlayerCmd.LoseGold))]
        public static class LoseGoldPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(decimal amount, Player player, GoldLossType goldLossType, out decimal __state)
            {
                __state=player.Gold;
                return true;
            }
            [HarmonyPostfix]
            public static async Task Postfix(Task __result,decimal amount, Player player, GoldLossType goldLossType,decimal __state)
            {
                await __result;
                decimal delta = __state - player.Gold;
                foreach (RelicModel relic in player.Relics.ToList())
                {
                    if (relic is SaisenBox saisenBox)
                    {
                        await saisenBox.LoseGold(player, delta, goldLossType);
                    }
                }
            }
        }
    }
}