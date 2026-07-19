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
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace HakureiReimu.HakureiReimuMod.Relics
{
    public class UltramarineOrbElixir:AbstractRelic
    {
        public override RelicRarity Rarity => RelicRarity.Rare;
        public List<CardModel> Cards = null;
        protected override void AfterCloned()
        {
            base.AfterCloned();
            Cards = null;
        }

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

        public static List<CardModel> GetDefaultCards(Player player)
        {
            return player.Character.StartingDeck.Select(c => player.RunState.CreateCard(c, player)).ToList();
        }

        public static CardModel CreateCard(Player owner, SerializableCard s)
        {
            try
            {
                CardModel card = CardModel.FromSerializable(s);
                card.FloorAddedToDeck = owner.RunState.ActFloor;
                owner.RunState.AddCard(card,owner);
                card.AfterCreated();
                return card;
            }
            catch
            {
                return null;
            }
        }
        
        public override bool IsAllowed(IRunState runState)
        {
            if (!RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
            {
                return true;
            }
            return TryGetCards(runState, LocalContext.GetMe(runState)) != null;
        }

        public override async Task AfterObtained()
        {
            if (!LocalContext.IsMe(Owner))return;
            bool canSkip = false;
            const int count = 1;
            List<CardModel> cards = TryGetCards(Owner.RunState,Owner);
            if (cards == null || cards.Count == 0)
            {
                canSkip = true;
                cards = GetDefaultCards(Owner);
            }
            if (cards == null || cards.Count == 0)
            {
                HakureiReimuMain.Logger.Info("绀珠之药获得时没有返回任何卡牌? 这是不应该出现的情况");
                return;
            }
            CardSelectorPrefs prefs = new(SelectionScreenPrompt, canSkip ? 0 : count, count)
            {
                Cancelable = false,
                RequireManualConfirmation = true
            };
            NPlayerHand.Instance?.CancelAllCardPlay();
            NSimpleCardSelectScreen screen = NSimpleCardSelectScreen.Create(cards, prefs);
            NOverlayStack.Instance?.Push(screen);
            CardModel c = (await screen.CardsSelected()).FirstOrDefault();
            // CardModel c=(await CardSelectCmd.FromSimpleGrid(new BlockingPlayerChoiceContext(), cards, Owner, prefs)).FirstOrDefault();
            if (c != null)
            {
                if (RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
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
                if (Card == null)
                {
                    HakureiReimuMain.Logger.Info("绀珠之药的同步动作出现错误! 无法反序列化Card!");
                    return;
                };
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