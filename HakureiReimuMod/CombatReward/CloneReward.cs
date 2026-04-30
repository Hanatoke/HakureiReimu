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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace HakureiReimu.HakureiReimuMod.CombatReward
{
    public class CloneReward(Player player) :AbstractReward(player)
    {
        public static readonly RewardType Type = (RewardType)CustomRewardPatch.GenerateEnum(typeof(CloneReward).FullName);
        protected override RewardType RewardType=>Type;
        public override int RewardsSetIndex => 12;
        public override bool IsPopulated => true;
        public override Task Populate()=>Task.CompletedTask;
        protected override string IconPath =>ImageHelper.GetImagePath($"ui/rest_site/option_{"CLONE".ToLowerInvariant()}.png");

        protected override async Task<bool> OnSelect()
        {
            CardModel card = (await CardSelectCmd.FromDeckGeneric(Player, new CardSelectorPrefs(Description, 1)
            {
                Cancelable = true
            })).FirstOrDefault();
            if (card != null)
            {
                if (RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
                {
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(Player.RunState.CloneCard(card),PileType.Deck));
                }
                else
                {
                    RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new CloneAction()
                    {
                        Player =  Player,
                        ModelId =  card.Id,
                        PileType = card.Pile?.Type??PileType.None,
                        Index = card.Pile?.Cards.IndexOf(card)??-1
                    });
                }
            }
            return card != null;
        }

        public override void MarkContentAsSeen()
        {
            
        }
        private struct NetCloneAction :INetAction
        {
            public int Index;
            public ModelId Id;
            public PileType PileType;
            public void Serialize(PacketWriter writer)
            {
                writer.WriteInt(Index);
                writer.WriteModelEntry(Id);
                writer.WriteEnum(PileType);
            }

            public void Deserialize(PacketReader reader)
            {
                Index = reader.ReadInt();
                Id = reader.ReadModelIdAssumingType<CardModel>();
                PileType = reader.ReadEnum<PileType>();
            }

            public GameAction ToGameAction(Player player)
            {
                return new CloneAction()
                {
                    Player = player,
                    Index = Index,
                    ModelId = Id,
                    PileType = PileType
                };
            }
        }
        private class CloneAction:GameAction
        {
            public Player Player;
            public int Index;
            public ModelId ModelId;
            public PileType PileType;
            protected override async Task ExecuteAction()
            {
                CardModel card = PileType.GetPile(Player).Cards[Index];
                if (card.Id==ModelId)
                {
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(Player.RunState.CloneCard(card),PileType.Deck));
                }
                else
                {
                    throw new SerializationException("ModelId " + ModelId + " with "+Index+" is not matched");
                }
            }

            public override INetAction ToNetAction()
            {
                return new NetCloneAction()
                {
                    Index = Index,
                    Id = ModelId,
                    PileType =  PileType
                };
            }
            public override ulong OwnerId => Player.NetId;
            public override GameActionType ActionType => GameActionType.Any;
        }
    }
}