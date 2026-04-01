using BaseLib.Patches.Content;
using HakureiReimu.HakureiReimuMod.PersistCard;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HakureiReimu.HakureiReimuMod.Cards
{
    public class CounterCardTable() :AbstractPersistCardTable(PileType)
    {
        [CustomEnum] 
        public static PileType PileType;
    }
}