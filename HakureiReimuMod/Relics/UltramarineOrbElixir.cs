using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace HakureiReimu.HakureiReimuMod.Relics
{
    public class UltramarineOrbElixir:AbstractRelic
    {
        public override RelicRarity Rarity => RelicRarity.Rare;
        public List<CardModel> Cards = null;

        public List<CardModel> TryGetCards(IRunState runState,Player player)
        {
            if (Cards != null) return Cards;
            foreach (string name in SaveManager.Instance.GetAllRunHistoryNames())
            {
                ReadSaveResult<RunHistory> read = SaveManager.Instance.LoadRunHistory(name);
                if (!read.Success || read.SaveData == null)continue;
                
                RunHistory data = read.SaveData;
                if (!data.Win||data.GameMode!=GameMode.Standard)continue;
                RunHistoryPlayer p = data.Players.FirstOrDefault(h=>h.Id==player.NetId);
                if (p == null)continue;
                if (p.Character != player.Character.Id) continue;
                List<CardModel> list = [];
                foreach (SerializableCard s in p.Deck)
                {
                    CardModel card=CreateCard(player, s);
                    if (card != null&&card.Rarity!=CardRarity.Event)
                    {
                        list.Add(card);
                    }
                }
                if (list.Count>0)
                {
                    return Cards=list;
                }
            }
            return null;
        }

        public static CardModel CreateCard(Player owner, SerializableCard s)
        {
            if (s.Id != null)
            {
                CardModel card = ModelDb.GetByIdOrNull<CardModel>(s.Id);
                if (card != null&&card.Rarity!=CardRarity.Event)
                {
                    card = owner.RunState.CreateCard(card, owner);
                    for (var i = 0; i < s.CurrentUpgradeLevel; i++)
                    {
                        CardCmd.Upgrade(card);
                    }
                    if (s.Enchantment?.Id!=null)
                    {
                        EnchantmentModel enchantment = ModelDb.GetByIdOrNull<EnchantmentModel>(s.Enchantment.Id)?.ToMutable();
                        if (enchantment != null)
                        {
                            CardCmd.Enchant(enchantment, card, s.Enchantment.Amount);
                        }
                    }
                    return card;
                }
            }
            return null;
        }
        
        public override bool IsAllowed(IRunState runState)
        {
            if (runState.CurrentRoom?.RoomType==RoomType.Treasure&&runState.Players.Count>1)
            {
                return false;
            }
            Player player = runState.GetPlayer(LocalContext.NetId.GetValueOrDefault());
            return TryGetCards(runState, player) != null;
        }

        public override async Task AfterObtained()
        {
            if (!LocalContext.IsMe(Owner))return;
            List<CardModel> cards = TryGetCards(Owner.RunState,Owner);
            if (cards == null || cards.Count == 0)
            {
                MainFile.Logger.Info("绀珠之药获得时没有返回任何卡牌? 这是不应该出现的情况");
                return;
            }
            CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1)
            {
                Cancelable = false
            };
            CardModel c=(await CardSelectCmd.FromSimpleGrid(new BlockingPlayerChoiceContext(), cards, Owner, prefs)).FirstOrDefault();
            if (c != null)
            {
                if (RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
                {
                    CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(c, PileType.Deck), 2);
                }
                else
                {
                    RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
                        new CardObtainAction(Owner,c));
                }
            }
        }
        private struct NetCardObtainAction :INetAction, IPacketSerializable
        {
            public SerializableCard Card;
            
            public void Serialize(PacketWriter writer)
            {
                Card.Serialize(writer);
            }

            public void Deserialize(PacketReader reader)
            {
                Card = new SerializableCard();
                Card.Deserialize(reader);
            }

            public GameAction ToGameAction(Player player)
            {
                return new CardObtainAction(player, CreateCard(player,Card));
            }
        }
        private class CardObtainAction :GameAction
        {
            public override ulong OwnerId =>Player.NetId;
            public override GameActionType ActionType => GameActionType.Any;
            public Player Player;
            public CardModel Card;

            public CardObtainAction(Player player, CardModel card)
            {
                Player = player;
                Card = card;
            }

            protected override async Task ExecuteAction()
            {
                
                var result=await CardPileCmd.Add(Card,PileType.Deck);
                if (LocalContext.IsMe(Player))
                {
                    CardCmd.PreviewCardPileAdd(result,2f);
                }
            }

            public override INetAction ToNetAction()
            {
                return new NetCardObtainAction()
                {
                    Card = Card.ToSerializable(),
                };
            }
        }
    }
}