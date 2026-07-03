using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class MakeNoException : AbstractCard
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain,CardKeyword.Exhaust];

        public MakeNoException(
            ) : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            Dictionary<PowerModel, int> debuffAmounts = cardPlay.Target.Powers
                .Where(p => p.TypeForCurrentAmount == PowerType.Debuff)
                .Select(p => ((PowerModel)p.ClonePreservingMutability(), p.Amount))
                .ToDictionary();
            foreach (KeyValuePair<PowerModel, int> pair in debuffAmounts)
            {
                if (pair.Key is ITemporaryPower temporaryPower)
                {
                    KeyValuePair<PowerModel, int> other =
                        debuffAmounts.FirstOrDefault(p => p.Key.Id == temporaryPower.InternallyAppliedPower.Id);
                    if (other.Key != null)
                    {
                        debuffAmounts[other.Key] += pair.Value;
                    }
                }
            }
            foreach (Creature enemy in CombatState.HittableEnemies)
            {
                if (enemy!=cardPlay.Target)
                {
                    foreach (KeyValuePair<PowerModel, int> pair in debuffAmounts)
                    {
                        if (pair.Value != 0)
                        {
                            PowerModel instanceForStacking = PowerCmd.FindExistingInstanceForStacking(pair.Key, enemy, pair.Key.Applier);
                            if (instanceForStacking != null)
                            {
                                await PowerCmd.ModifyAmount(choiceContext, instanceForStacking, pair.Value, pair.Key.Applier, this);
                            }
                            else
                                await PowerCmd.Apply(choiceContext, (PowerModel) pair.Key.ClonePreservingMutability(), enemy, pair.Value, pair.Key.Applier,this);
                        }
                    }
                }
            }
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
