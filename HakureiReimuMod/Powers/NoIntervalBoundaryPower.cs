using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards.Skill.Rare;
using HakureiReimu.HakureiReimuMod.Character;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class NoIntervalBoundaryPower : AbstractPower
    {
        public static readonly string ID = nameof(NoIntervalBoundaryPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public static bool Filter(CardModel card) => card.HasCounter() && card is not DreamInnate;
        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player != Owner.Player) return;
            Flash();
            List<CardModel> cards = ModelDb.CardPool<HakureiReimuCardPool>()
                .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
                .Where(Filter).ToList();
            for (var i = 0; i < Amount; i++)
            {
                CardModel card = CardFactory.GetDistinctForCombat(player,
                        cards, 1, player.RunState.Rng.CombatCardGeneration)
                    .FirstOrDefault();
                if (card != null)
                {
                    await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
                }
            }
        }
    }
}