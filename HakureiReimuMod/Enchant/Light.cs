using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HakureiReimu.HakureiReimuMod.Enchant
{
    public class Light :AbstractEnchantment
    {
        public override bool ShowAmount => Amount > 1;
        public override bool HasExtraCardText => true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Energy)];
        public override bool CanEnchant(CardModel card)
        {
            return card.Enchantment == null || this.IsStackable && !(card.Enchantment.GetType() != this.GetType());
        }

        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (this.Status!=EnchantmentStatus.Normal||card != this.Card || this.Card.Pile?.Type != PileType.Hand)return;
            this.Status = EnchantmentStatus.Disabled;
            await PlayerCmd.GainEnergy(Amount, card.Owner);
            NCard.FindOnTable(card)?.CardHighlight.AnimFlash();
        }

        public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel source)
        {
            if (card==this.Card&&oldPileType==PileType.Hand&&this.Status==EnchantmentStatus.Disabled)
            {
                CardCmd.ClearEnchantment(card);
            }
            return Task.CompletedTask;
        }
    }
}