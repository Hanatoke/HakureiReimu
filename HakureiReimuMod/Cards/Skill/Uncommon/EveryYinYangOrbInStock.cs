using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class EveryYinYangOrbInStock : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
                new RepeatVar(5)
            ];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [HoverTipFactory.FromOrb<YinYangOrb>()];

        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

        public EveryYinYangOrbInStock(
            ) : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies) {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int n = DynamicVars.Repeat.IntValue;
            foreach (Player t in CombatState.Players.Where(p=>p is {Creature.IsAlive: true }).ToList())
            {
                await YinYangOrbCmd.Spawn(choiceContext, t, n, this);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Repeat.UpgradeValueBy(2);
        }
    }
}
