using HakureiReimu.HakureiReimuMod.PersistCard;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards
{
    public class NoCountCardSlot :AbstractPersistCardSlot
    {
        public NoCountCardSlot(CardModel card) : base(card, 0)
        {
        }

        public override bool ShouldDisplayCount =>false;
    }
}