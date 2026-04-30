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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace HakureiReimu.HakureiReimuMod.CombatReward
{
    public class TransformReward(Player player) :AbstractReward(player)
    {
        public static readonly RewardType Type = (RewardType)CustomRewardPatch.GenerateEnum(typeof(TransformReward).FullName);
        protected override RewardType RewardType=>Type;
        public override int RewardsSetIndex => 11;
        public override bool IsPopulated => true;
        public override Task Populate()=>Task.CompletedTask;
        // protected override string IconPath =>ImageHelper.GetImagePath($"ui/rest_site/option_{"SMITH".ToLowerInvariant()}.png");

        protected override async Task<bool> OnSelect()
        {
            CardModel card = (await CardSelectCmd.FromDeckForTransformation(Player, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1)
            {
                Cancelable = true
            })).FirstOrDefault();
            if (card != null)
            {
                if (RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
                {
                    await CardCmd.TransformToRandom(card, Player.PlayerRng.Transformations,CardPreviewStyle.EventLayout);
                }
                else
                {
                    RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new TransformAction()
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
        private struct NetTransformAction :INetAction
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
                return new TransformAction()
                {
                    Player = player,
                    Index = Index,
                    ModelId = Id,
                    PileType = PileType
                };
            }
        }
        private class TransformAction:GameAction
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
                    await CardCmd.TransformToRandom(card, Player.PlayerRng.Transformations,CardPreviewStyle.EventLayout);
                }
                else
                {
                    throw new SerializationException("ModelId " + ModelId + " with "+Index+" is not matched");
                }
            }

            public override INetAction ToNetAction()
            {
                return new NetTransformAction()
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