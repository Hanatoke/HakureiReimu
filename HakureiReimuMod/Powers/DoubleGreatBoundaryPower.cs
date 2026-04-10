using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class DoubleGreatBoundaryPower : AbstractPower
    {
        public static readonly string ID = nameof(DoubleGreatBoundaryPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public override int ModifyCardPlayCount(CardModel card, Creature target, int playCount)
        {
            if (card.Owner.Creature==Owner&&card.HasCounter())
            {
                return playCount+Amount;
            }
            return playCount;
        }
    }
}