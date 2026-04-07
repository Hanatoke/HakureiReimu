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
            List<PowerModel> originalDebuffs = cardPlay.Target.Powers.Where(p => p.TypeForCurrentAmount == PowerType.Debuff)
                .Select(p => (PowerModel) p.ClonePreservingMutability()).ToList();
            foreach (Creature enemy in CombatState.HittableEnemies)
            {
                if (enemy!=cardPlay.Target)
                {
                    foreach (PowerModel p in originalDebuffs)
                    {
                        PowerModel powerById = enemy.GetPowerById(p.Id);
                        if (powerById != null && !powerById.IsInstanced)
                        {
                            DoHackyThingsForSpecificPowers(powerById);
                            await PowerCmd.ModifyAmount(powerById, p.Amount, Owner.Creature, this);
                        }
                        else
                        {
                            PowerModel power =(PowerModel)p.ClonePreservingMutability();
                            DoHackyThingsForSpecificPowers(power);
                            await PowerCmd.Apply(power, enemy,p.Amount, Owner.Creature, this);
                        }
                    }
                }
            }
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
        private static void DoHackyThingsForSpecificPowers(PowerModel power)
        {
            if (!(power is ITemporaryPower temporaryPower))
                return;
            temporaryPower.IgnoreNextInstance();
        }
    }
}
