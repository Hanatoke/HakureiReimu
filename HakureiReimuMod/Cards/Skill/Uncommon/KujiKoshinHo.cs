using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class KujiKoshinHo : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new BlockVar(9,ValueProp.Move),new PowerVar<KujiKoshinHoPower>(1)];
        public KujiKoshinHo(
            ) : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        }
        public override bool GainsBlock => true;

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            await PowerCmd.Apply<KujiKoshinHoPower>(Owner.Creature, DynamicVars[KujiKoshinHoPower.ID].IntValue,
                Owner.Creature,
                this);
        }

        protected override void OnUpgrade() => this.DynamicVars.Block.UpgradeValueBy(3);
    }
}
