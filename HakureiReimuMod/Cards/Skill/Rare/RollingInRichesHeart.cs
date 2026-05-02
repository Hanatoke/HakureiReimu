using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.CombatReward;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class RollingInRichesHeart : AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(50)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal,CardKeyword.Exhaust];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellFastA;

        public RollingInRichesHeart(
            ) : base(0, CardType.Skill, CardRarity.Rare, TargetType.None) {
        }

        private static LocString _talk;
        public LocString Talk => _talk ??= new LocString("cards", this.Id.Entry + ".talk");
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int gold = DynamicVars.Gold.IntValue;
            if (Owner.Gold>=gold)
            {
                PlayerCmd.LoseGold(gold, Owner, GoldLossType.Spent);
                (RunState?.CurrentRoom as CombatRoom)?.AddExtraReward(Owner,new BlindBoxReward(Owner));
            }
            else
            {
                TalkCmd.Play(Talk, Owner.Creature, VfxColor.White);
            }
            return Task.CompletedTask;
        }
        protected override void OnUpgrade() {
            DynamicVars.Gold.UpgradeValueBy(-10);
        }
    }
}
