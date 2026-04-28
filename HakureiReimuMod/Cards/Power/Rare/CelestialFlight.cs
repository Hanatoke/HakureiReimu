using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Enchant;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Rare {
    public class CelestialFlight : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Light>();

        public CelestialFlight(
            ) : base(3, CardType.Power, CardRarity.Rare, TargetType.Self) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            List<CardModel> cards = Owner.PlayerCombatState?.DrawPile.Cards.ToList();
            if (cards == null || cards.Count == 0)return Task.CompletedTask;
            CardPreviewStyle style = cards.Count > 5 ? CardPreviewStyle.MessyLayout : CardPreviewStyle.HorizontalLayout;
            int n = 20;
            foreach (CardModel c in cards)
            {
                if (c.Enchantment!=null)
                {
                    if (c.Enchantment is Light light)
                    {
                        light.Amount++;
                        light.ModifyCard();
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    CardCmd.Enchant<Light>(c, 1);
                }
                if (n>0)
                {
                    n--;
                    CardCmd.Preview(c, 2f, style);
                }
            }
            return Task.CompletedTask;
        }
        protected override void OnUpgrade() 
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
