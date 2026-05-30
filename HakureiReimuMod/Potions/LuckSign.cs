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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Potions
{
    public class LuckSign :AbstractPotion
    {
        public override PotionRarity Rarity => PotionRarity.Rare;
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override TargetType TargetType => TargetType.Self;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature target)
        {
            List<CardModel> cards = Owner.Character.CardPool
                .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint).Where(c =>
                    c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
                .Select(c =>
                {
                    c = (CardModel)c.MutableClone();
                    c.Owner = this.Owner;
                    return c;
                })
                .ToList();
            cards = new CardAnalyzer(Owner.Creature.CombatState, Owner, cards){VerifyResource = false}.Analyze()
                .GetResultsByMost(Owner.RunState.Rng.CombatCardGeneration, DynamicVars.Cards.IntValue);
            await CardPileCmd.AddGeneratedCardsToCombat(cards.Select(c =>
            {
                c = Owner.Creature.CombatState.CreateCard(c.CanonicalInstance, Owner);
                c.SetToFreeThisTurn();
                return c;
            }).ToList(), PileType.Hand, Owner);
        }
    }
}