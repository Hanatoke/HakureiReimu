using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Potions
{
    public class BottledBoundary :AbstractPotion
    {
        public override PotionRarity Rarity => PotionRarity.Uncommon;
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override TargetType TargetType => TargetType.Self;

        public override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromKeyword(AbstractCard.Counter)
        ];

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature target)
        {
            CardModel card = await CardSelectCmd.FromChooseACardScreen(choiceContext,
                CardFactory.GetDistinctForCombat(Owner,
                    Owner.Character.CardPool
                        .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
                        .Where(c => c.HasCounter()), 3, Owner.RunState.Rng.CombatCardGeneration).ToList(), Owner, true);
            if (card!=null)
            {
                card.SetToFreeThisCombat();
                await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true);
            }
        }
    }
}