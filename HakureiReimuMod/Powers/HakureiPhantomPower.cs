using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.Node.VFX.Special;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Interface;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class HakureiPhantomPower : AbstractPower,IPersistCardSubscriber
    {
        public static readonly string ID = nameof(HakureiPhantomPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
        ];

        protected FollowVFX Vfx;
        protected override void AfterCloned()
        {
            base.AfterCloned();
            Vfx = null;
        }

        public override int ModifyCardPlayCount(CardModel card, Creature target, int playCount)
        {
            if (card.Owner.Creature == this.Owner)
            {
                return playCount + Amount;
            }
            return playCount;
        }

        public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay,
            ResourceInfo resources, PileType pileType, CardPilePosition position)
        {
            if (card.Owner.Creature!=Owner)
            {
                return (pileType, position);
            }
            return (PileType.Exhaust, position);
        }

        public PileType ModifyStopPersistCardPile(AbstractPersistCardSlot slot, PileType defaultPile)
        {
            if (slot.Card?.Owner.Creature == this.Owner)
            {
                return PileType.Exhaust;
            }
            return defaultPile;
        }

        public override Task AfterApplied(Creature applier, CardModel cardSource)
        {
            Vfx?.QueueFreeSafely();
            NCreature owner = Owner.GetCreatureNode();
            Control container = Owner.GetVfxContainer();
            if (owner != null && container != null)
            {
                Vfx = FollowVFX.Create(() => GodotObject.IsInstanceValid(owner) ? owner.VfxSpawnPosition : null,0,0,0,0);
                NPhantom phantom = NPhantom.Create(this.Amount);
                Vfx.AddChildSafely(phantom);
                phantom.Modulate = Colors.DarkViolet;
                phantom.Scale = 1.5f * Vector2.One;
                container.AddChildSafely(Vfx);

                FlyingVFXCmd.AddVFXOnTarget(NNova.Create(3),owner.VfxSpawnPosition,container);
            }
            return Task.CompletedTask;
        }
        public override Task AfterRemoved(Creature oldOwner)
        {
            Vfx?.QueueFreeSafely();
            return Task.CompletedTask;
        }

        public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature applier,
            CardModel cardSource)
        {
            if (power==this && Vfx?.GetChildren().FirstOrDefault(n=>n is NPhantom) is NPhantom phantom)
            {
                phantom.SetCount(this.Amount);
            }
            return Task.CompletedTask;
        }
    }
}