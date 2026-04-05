using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards.Attack.Common;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class YinYangPreciousJade : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [new CalculationBaseVar(0),new CalculationExtraVar(1)
                ,new CalculatedVar("CalculatedNum").WithMultiplier((c,_)=>
                {
                    YinYangOrbManager manager = c.Owner.PlayerCombatState.YinYangOrbManager();
                    if (manager!=null)
                    {
                        return manager.Capacity - manager.Orbs.Count;
                    }
                    return 0;
                })];

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [HoverTipFactory.FromOrb<YinYangOrb>()];

        public YinYangPreciousJade(
            ) : base(1, CardType.Skill, CardRarity.Common, TargetType.None) {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int n = (int)((CalculatedVar)DynamicVars["CalculatedNum"]).Calculate(cardPlay.Target);
            await YinYangOrbCmd.Spawn(choiceContext, Owner, n);
        }

        protected override void OnUpgrade()
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
    }
}
