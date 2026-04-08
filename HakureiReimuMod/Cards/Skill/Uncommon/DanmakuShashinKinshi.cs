using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class DanmakuShashinKinshi : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
                new BlockVar(5,ValueProp.Move),
                new PowerVar<InhibitPower>(1),
                new CalculationBaseVar(0),
                new CalculationExtraVar(1),
                new CalculatedVar("CalculatedTimes").WithMultiplier((c, _) =>
                    c.CombatState?.HittableEnemies.Count(e => e.Monster is { IntendsToAttack: true })??0)
            ];

        public override bool GainsBlock => true;

        public DanmakuShashinKinshi(
            ) : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int n = (int)((CalculatedVar)DynamicVars["CalculatedTimes"]).Calculate(cardPlay.Target);
            for (var i = 0; i < n; i++)
            {
                await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            }
            foreach (Creature t in CombatState.HittableEnemies)
            {
                if (t.Monster is { IntendsToAttack: false })
                {
                    await PowerCmd.Apply<InhibitPower>(t, DynamicVars[InhibitPower.ID].IntValue, Owner.Creature, this);
                }
            }
        }
        protected override void OnUpgrade() {
            DynamicVars.Block.UpgradeValueBy(2);
            DynamicVars[InhibitPower.ID].UpgradeValueBy(1);
        }
    }
}
