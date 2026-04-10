using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class SprinkleStarSeal : AbstractCard
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public SprinkleStarSeal(
            ) : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy) {
        }
        public static readonly HashSet<Type> Special = [
            typeof(HardenedShellPower),
            typeof(HardToKillPower),
        ];
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            foreach (PowerModel p in cardPlay.Target.Powers)
            {
                if (!p.IsVisible)continue;
                if (p.Type==PowerType.Buff&&!Special.Contains(p.GetType()))
                {
                    await PowerCmd.Apply((PowerModel)p.ClonePreservingMutability(), cardPlay.Target,
                        -(int)MathF.Floor(p.Amount / 2f), Owner.Creature, this);
                }
                else if (p.Type != PowerType.None)
                {
                    await PowerCmd.Apply((PowerModel)p.ClonePreservingMutability(), cardPlay.Target, p.Amount,
                        Owner.Creature, this);
                }
            }
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
