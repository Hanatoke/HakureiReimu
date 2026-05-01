using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Patches;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace HakureiReimu.HakureiReimuMod.CombatReward
{
    public class EnchantReward(Player player,ModelId enchantId,int amount=1) :AbstractReward(player)
    {
        public static readonly RewardType Type = (RewardType)CustomRewardPatch.GenerateEnum(typeof(EnchantReward).FullName);
        protected override RewardType RewardType=>Type;
        public override int RewardsSetIndex => 20;
        public ModelId EnchantId = enchantId;
        public int Amount=amount;
        public EnchantmentModel Enchantment=null;
        public override bool IsPopulated => Enchantment!=null;
        public override Task Populate()
        {
            Enchantment = ModelDb.GetByIdOrNull<EnchantmentModel>(EnchantId)?.ToMutable();
            if (Enchantment == null)throw new ArgumentException("enchantment not found");
            Enchantment.Amount=Amount;
            Enchantment.RecalculateValues();
            return Task.CompletedTask;
        }

        protected override string IconPath => Enchantment?.IconPath;
        public override IEnumerable<IHoverTip> HoverTips =>Enchantment?.HoverTips??[];

        public override LocString Description
        {
            get
            {
                LocString loc = DefaultDescription;
                if (Enchantment != null)
                {
                    loc.Add("EnchantName",Enchantment.Title);
                    loc.Add("Amount",Amount);
                }
                return loc;
            }
        }

        protected override async Task<bool> OnSelect()
        {
            List<CardModel> cards = PileType.Deck.GetPile(player).Cards
                .Where((Func<CardModel, bool>)(Enchantment.CanEnchant)).ToList();
            Dictionary<CardModel, int> indexMap = PileType.Deck.GetPile(player).Cards.Select((card, index) => new { card, index }).ToDictionary(x => x.card, x => x.index);
            List<CardModel> list = cards.OrderBy((Func<CardModel, int>) (c => indexMap[c])).ToList();
            if (list.Count <= 0) return true;
            CardModel card = (await NDeckEnchantSelectScreen.ShowScreen(list,Enchantment.CanonicalInstance, Amount,
                new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1)
                {
                    Cancelable = true
                }).CardsSelected()).FirstOrDefault();
            // CardModel card = (await CardSelectCmd.FromDeckForEnchantment(Player,Enchantment.CanonicalInstance,Amount, new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1)
            // {
            //     Cancelable = true
            // })).FirstOrDefault();
            if (card != null)
            {
                if (RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
                {
                    CardCmd.Enchant(Enchantment, card, Amount);
                    CardCmd.Preview(card);
                }
                else
                {
                    RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new EnchantAction()
                    {
                        Player =  Player,
                        ModelId =  card.Id,
                        PileType = card.Pile?.Type??PileType.None,
                        Index = card.Pile?.Cards.IndexOf(card)??-1,
                        EnchantId = EnchantId,
                        Amount = Amount
                    });
                }
            }
            return card != null;
        }

        public override void MarkContentAsSeen()
        {
            
        }

        public override SerializableReward ToSerializable()
        {
            return new SerializableReward()
            {
                RewardType = this.RewardType,
                OptionCount = this.Amount,
                PredeterminedModelId = EnchantId
            };
        }

        private struct NetEnchantAction :INetAction
        {
            public int Index;
            public ModelId Id;
            public PileType PileType;
            public ModelId EnchantId;
            public int Amount;
            public void Serialize(PacketWriter writer)
            {
                writer.WriteInt(Index);
                writer.WriteModelEntry(Id);
                writer.WriteEnum(PileType);
                writer.WriteModelEntry(EnchantId);
                writer.WriteInt(Amount);
            }

            public void Deserialize(PacketReader reader)
            {
                Index = reader.ReadInt();
                Id = reader.ReadModelIdAssumingType<CardModel>();
                PileType = reader.ReadEnum<PileType>();
                EnchantId = reader.ReadModelIdAssumingType<EnchantmentModel>();
                Amount = reader.ReadInt();
            }

            public GameAction ToGameAction(Player player)
            {
                return new EnchantAction()
                {
                    Player = player,
                    Index = Index,
                    ModelId = Id,
                    PileType = PileType,
                    EnchantId =  EnchantId,
                    Amount =  Amount
                };
            }
        }
        private class EnchantAction:GameAction
        {
            public Player Player;
            public int Index;
            public ModelId ModelId;
            public PileType PileType;
            public ModelId EnchantId;
            public int Amount;
            protected override Task ExecuteAction()
            {
                CardModel card = PileType.GetPile(Player).Cards[Index];
                if (card.Id==ModelId)
                {
                    EnchantmentModel mutable = ModelDb.GetByIdOrNull<EnchantmentModel>(EnchantId)?.ToMutable();
                    if (mutable==null)throw new SerializationException("EnchantmentModel not found");
                    mutable.Amount = Amount;
                    mutable.RecalculateValues();
                    CardCmd.Enchant(mutable, card, Amount);
                    CardCmd.Preview(card);
                }
                else
                {
                    throw new SerializationException("ModelId " + ModelId + " with "+Index+" is not matched");
                }
                return Task.CompletedTask;
            }

            public override INetAction ToNetAction()
            {
                return new NetEnchantAction()
                {
                    Index = Index,
                    Id = ModelId,
                    PileType =  PileType,
                    EnchantId =  EnchantId,
                    Amount =   Amount
                };
            }
            public override ulong OwnerId => Player.NetId;
            public override GameActionType ActionType => GameActionType.Any;
        }
    }
}