using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class HakureiPhantomPower : AbstractPower
    {
        public static readonly string ID = nameof(HakureiPhantomPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
        ];

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
    }
}