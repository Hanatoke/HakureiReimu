using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class DivinePossessionPower : AbstractPower
    {
        public static readonly string ID = nameof(DivinePossessionPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        
        public override async Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
        {
            if (player!=Owner.Player)return;
            CardPile pile = PileType.Hand.GetPile(Owner.Player);
            if (!pile.Cards.Any(c => c.CanPlay())) return;
            Flash();
            using (CardSelectCmd.PushSelector(new CardSelector.CombatSmartSelector(choiceContext,Owner.Player,Owner.Player.RunState.Rng.CombatTargets)))
            {
                CardAnalyzer.WeightSetting setting = new()
                {
                    DrawCardWeight = -1,
                    GainEnergyWeight = 0,
                    CardCostMulti = -0.5m,
                };
                for (var i = 0; i < Amount; i++)
                {
                    if (CombatManager.Instance.IsOverOrEnding)return;
                    if (pile.IsEmpty) return;
                    CardAnalyzer analyzer = new CardAnalyzer(Owner.CombatState, Owner.Player,
                        pile.Cards.Where(c => c.CanPlay()).ToList())
                        {
                            CalculateByVirtual = false,
                            VerifyResource = false,
                            Setting = setting
                        }
                        .Analyze();
                    List<CardModel> cards = analyzer.GetResultsByBest(Owner.Player.RunState.Rng.CombatTargets, 1);
                    if (cards.Count > 0)
                    {
                        await CardCmd.AutoPlay(choiceContext, cards[0], analyzer.CardTarget.GetValueOrDefault(cards[0], null));
                    }
                }
            }
        }
    }
}