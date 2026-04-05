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
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.Seal>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new PowerVar<Powers.Seal>(7)];
        
        public MushabetsuChofuku(
            ) : base(1, CardType.Skill, CardRarity.Common, TargetType.AllAllies) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            foreach (Creature t in CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<Powers.Seal>(t, DynamicVars[Powers.Seal.ID].BaseValue,
                    Owner.Creature, this);
            }
        }
        protected override void OnUpgrade() {
            DynamicVars[Powers.Seal.ID].UpgradeValueBy(3);
        }
    }
}
