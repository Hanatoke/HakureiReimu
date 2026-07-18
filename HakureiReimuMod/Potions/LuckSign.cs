using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Potions
{
    public class LuckSign :AbstractPotion
    {
        public override PotionRarity Rarity => PotionRarity.Rare;
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override TargetType TargetType => TargetType.AnyPlayer;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature target)
        {
            AssertValidForTargetedPotion(target);
            Player player = target.Player;
            List<CardModel> cards = player.Character.CardPool
                .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint).Where(c =>
                    c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
                .Select(c =>
                {
                    try
                    {
                        c = (CardModel)c.MutableClone();
                        c.Owner = player;
                        return c;
                    }catch { return null; }
                }).Where(c=>c!=null).ToList();
            cards = new CardAnalyzer(player.Creature.CombatState, player, cards){VerifyResource = false}.Analyze()
                .GetResultsByMost(player.RunState.Rng.CombatCardGeneration, DynamicVars.Cards.IntValue);
            await CardPileCmd.AddGeneratedCardsToCombat(cards.Select(c =>
            {
                c = player.Creature.CombatState.CreateCard(c.CanonicalInstance, player);
                c.SetToFreeThisTurn();
                return c;
            }).ToList(), PileType.Hand, Owner);
        }
    }
}