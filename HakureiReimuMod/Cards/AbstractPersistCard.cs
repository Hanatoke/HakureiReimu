using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Node;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using HakureiReimu.HakureiReimuMod.PersistCard.Interface;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HakureiReimu.HakureiReimuMod.Cards
{
    public abstract class AbstractPersistCard(int cost, CardType type, CardRarity rarity, TargetType target) 
        :AbstractCard(cost, type, rarity, target),IPersistCard,IPersistCardSubscriber
    {
        // public Vector2 PersistIconOffset = new Vector2(115, -120);
        public Vector2 PersistIconOffset = new Vector2(-150, -210);
        public CardModel Model => this;
        public AbstractPersistCardSlot InstanceSlot =>Slot=new CounterCardSlot(this,Duration);
        public abstract int Duration { get; }
        public virtual PileType TargetPersistPileType => CounterCardTable.PileType;
        // public bool InPersisting => TargetPersistPileType.GetPile(Owner).Cards.IndexOf(this) >= 0;
        public bool InPersisting{ get; protected set; }
        public AbstractPersistCardSlot Slot=null;
        public static readonly Dictionary<ModelId, int> CounterActivates = new();
        
        public int ActivateThisTurn
        {
            get { return CounterActivates.GetValueOrDefault(Id, 0); }
            set { CounterActivates[Id]=value; }
        }
        //-------------------------------------------------------------------------------------------------
        public override void OnReload(NCard card)
        {
            base.OnReload(card);
            Control persistIcon = NPersistIcon.Create();
            card.Body.AddChildSafely(persistIcon);
            persistIcon.Position = PersistIconOffset;
            card.ShowUpgradePreview();
        }
        //-------------------------------------------------------------------------------------------------

        protected override PileType GetResultPileType()
        {
            if (Pile?.Type!=CounterCardTable.PileType)
            {
                return TargetPersistPileType;
            }
            return base.GetResultPileType();
        }

        public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay,
            ResourceInfo resources, PileType pileType, CardPilePosition position)
        {
            if (card==this&&Pile?.Type!=CounterCardTable.PileType)
            {
                return (TargetPersistPileType, position);
            }
            return (pileType, position);
        }

        public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player==Owner)
            {
                CounterActivates.Clear();
            }
            return Task.CompletedTask;
        }

        public Task OnStartPersistCard(AbstractPersistCardSlot slot)
        {
            if (slot.Card==this)
            {
                InPersisting = true;
            }
            return Task.CompletedTask;
        }
        
        public Task OnStopPersistCard(AbstractPersistCardSlot slot)
        {
            if (slot.Card == this)
            {
                InPersisting = false;
            }
            return Task.CompletedTask;
        }

        public async Task AfterModifyPersistCount(AbstractPersistCardSlot slot, int result)
        {
            if (slot.Card == this&&result<=0)
            {
                await PersistCardCmd.StopPersistCard(slot);
            }
        }

        public virtual async Task Flash(bool instant = false)
        {
            if (InPersisting&&Slot!=null)
            {
                float delay = ActivateThisTurn switch
                {
                    0 => 0.8f,
                    1 => 0.5f,
                    2 => 0.25f,
                    _ => 0
                };
                if (instant) delay = 0;
                ActivateThisTurn++;
                try
                {
                    await PersistCardCmd.FlashPersistCard(Slot, delay);
                }
                catch (Exception e)
                {
                    MainFile.Logger.Warn(e.ToString());
                }
            }
        }

        public virtual async Task Decrement()
        {
            if (InPersisting&&Slot!=null)
            {
                await PersistCardCmd.DecreaseCount(Slot, 1);
            }
        }
    }
}