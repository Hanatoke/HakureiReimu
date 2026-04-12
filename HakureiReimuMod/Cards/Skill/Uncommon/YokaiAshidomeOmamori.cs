using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class YokaiAshidomeOmamori : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => 
            [HoverTipFactory.FromPower<SealPower>(),HoverTipFactory.Static(StaticHoverTip.Stun)];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new PowerVar<SealPower>(20)];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellFastA;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
        public LocString Talk => LocString.GetIfExists("cards", Id.Entry + ".talk");
        public YokaiAshidomeOmamori(
            ) : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target.GetPowerAmount<SealPower>()>=DynamicVars[SealPower.ID].IntValue)
            {
                await CreatureCmd.Stun(cardPlay.Target);
            }
            else
            {
                TalkCmd.Play(Talk, Owner.Creature,vfxColor:VfxColor.White);
            }
        }
        protected override void OnUpgrade() {
            DynamicVars[SealPower.ID].UpgradeValueBy(-5);
        }
    }
}
