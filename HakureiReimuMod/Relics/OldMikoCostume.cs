using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Cards.Skill.Common;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.Relics
{
    public class OldMikoCostume:AbstractRelic
    {
        public override RelicRarity Rarity => RelicRarity.Common;
        private CardModel _card;
        public CardModel Card
        {
            get
            {
                if (_card == null)
                {
                    _card = ((CardModel)ModelDb.Card<DoubleBoundary>().MutableClone());
                    _card.AddKeyword(CardKeyword.Exhaust);
                }
                return _card;
            }
        }
        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip> {
            HoverTipFactory.FromCard(Card),
        }.Concat(Card.HoverTips);

        public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
        {
            if (player==Owner&&player.Creature.CombatState?.RoundNumber==1)
            {
                Flash();
                DoubleBoundary card = (DoubleBoundary)player.Creature.CombatState.CreateCard<DoubleBoundary>(Owner).CreateDupe();
                card.AddKeyword(CardKeyword.Exhaust);
                await CardPileCmd.Add(card, PileType.Play, skipVisuals: true);
                if (LocalContext.IsMe(Owner)&&NCombatRoom.Instance is {} room)
                {
                    NCard nCard=NCard.Create(card);
                    room.Ui.AddChildSafely(nCard);
                    NRelicInventoryHolder nR = NRun.Instance?.GlobalUi.RelicInventory.RelicNodes.FirstOrDefault(r=>r.Relic.Model==this);
                    if (nR != null)
                    {
                        nCard.GlobalPosition = nR.GlobalPosition;
                        nCard.Scale = Vector2.Zero;
                    }
                    else
                    {
                        nCard.Position = room.Ui.GetViewportRect().GetCenter();
                    }
                }
                await PersistCardCmd.StartPersistCard(
                    card.TargetPersistPileType.GetPile(Owner) as AbstractPersistCardTable, card.InstanceSlot);
            }
        }
    }
}