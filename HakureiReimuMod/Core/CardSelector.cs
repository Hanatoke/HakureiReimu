using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public static class CardSelector
    {
        public class CombatSmartSelector : ICardSelector
        {
            public PlayerChoiceContext ChoiceContext;
            public Player Owner;
            public Rng Rng;
            public ICombatState CombatState => Owner.Creature.CombatState;

            public CombatSmartSelector(PlayerChoiceContext choiceContext,Player player,Rng rng)
            {
                ChoiceContext = choiceContext;
                Owner = player;
                Rng = rng;
                if (player.Creature.CombatState == null) throw new RuntimeWrappedException("Must be used in Combat");
            }

            public bool IsNegativeChoose(CardModel card) => card.IsExhaustCard() || card.IsDiscardCard();

            public Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
            {
                CardModel card = ChoiceContext?.ModelStack?.FirstOrDefault(a=>a is CardModel) as CardModel;
                bool isNegative = card != null && IsNegativeChoose(card);
                Func<CardAnalyzer,CardModel, int> modifier = null;
                if (isNegative && card.IsDiscardCard())
                {
                    modifier = (_, c) => c.IsSlyThisTurn ? -200 : 0;
                }
                CardAnalyzer analyzer = new CardAnalyzer(CombatState, Owner,options.ToList())
                {
                    CalculateByVirtual = false,
                    VerifyResource = false,
                }.Analyze(modifier);
                int shouldSelect = isNegative
                    ? Math.Clamp(analyzer.Weights.Count(p => p.Value <= -100), minSelect, maxSelect)
                    : maxSelect;
                return Task.FromResult<IEnumerable<CardModel>>(analyzer.GetResultsByBest(Rng, shouldSelect, isNegative).ToList());
            }

            public CardRewardSelection GetSelectedCardReward(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> alternatives)
            {
                if (options.Count>0 && options.All(o=>o.Card.IsInCombat))
                {
                    CardModel card =
                        new CardAnalyzer(CombatState, Owner,
                                options.Select(o => o.Card).ToList())
                            {
                                CalculateByVirtual = false,
                                VerifyResource = false,
                            }.Analyze()
                            .GetResultsByBest(Rng, 1).FirstOrDefault();
                    return new CardRewardSelection()
                    {
                        card = card,
                        alternative = null
                    };
                }
                return new CardRewardSelection()
                {
                    card = options.FirstOrDefault()?.Card,
                    alternative = null
                };
            }
        }
    }
}