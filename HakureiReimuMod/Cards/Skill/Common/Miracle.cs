using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common
{
    [Pool(typeof(ColorlessCardPool))]
    public class Miracle :AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

        public Miracle() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Energy.UpgradeValueBy(1);
        }

        public override void OnReload(NCard card)
        {
            
        }
    }
}