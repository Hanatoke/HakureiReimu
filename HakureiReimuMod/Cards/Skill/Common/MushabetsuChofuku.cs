using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class MushabetsuChofuku : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.SealPower>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new PowerVar<Powers.SealPower>(7)];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.AttackDashLight;
        public MushabetsuChofuku(
            ) : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            foreach (Creature t in CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<Powers.SealPower>(t, DynamicVars[Powers.SealPower.ID].BaseValue,
                    Owner.Creature, this);
            }
        }
        protected override void OnUpgrade() {
            DynamicVars[Powers.SealPower.ID].UpgradeValueBy(3);
        }
    }
}
