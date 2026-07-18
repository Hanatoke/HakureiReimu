using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards;
using HakureiReimu.HakureiReimuMod.Character;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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
        public override TargetType TargetType => TargetType.AnyPlayer;

        public override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromKeyword(AbstractCard.Counter)
        ];

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature target)
        {
            AssertValidForTargetedPotion(target);
            Player player = target.Player;
            CardModel card = await CardSelectCmd.FromChooseACardScreen(choiceContext,
                CardFactory.GetDistinctForCombat(player,
                    ModelDb.CardPool<HakureiReimuCardPool>()
                        .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
                        .Where(c => c.HasCounter()), 3, player.RunState.Rng.CombatCardGeneration).ToList(), player, true);
            if (card!=null)
            {
                card.SetToFreeThisCombat();
                await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
            }
        }
    }
}