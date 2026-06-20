using BaseLib.Patches.Content;
using HakureiReimu.HakureiReimuMod.PersistCard;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace HakureiReimu.HakureiReimuMod.Cards
{
    public class CounterCardTable() :AbstractPersistCardTable(PileType)
    {
        [CustomEnum] 
        public static PileType PileType;

        private LocString _pileViewScreenText;

        public override LocString PileViewScreenText(NCardPileScreen screen)
        {
            _pileViewScreenText ??= new LocString("gameplay_ui", "HAKUREIREIMU-COUNTER_CARD_TABLE_VIEW.description");
            _pileViewScreenText.Add("Amount",screen.Pile.Cards.Count);
            return _pileViewScreenText;
        }
    }
}