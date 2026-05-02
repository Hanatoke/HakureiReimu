using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class JumpTheGun : AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DamageVar(10, ValueProp.Move),
                new PowerVar<StrengthPower>(1)
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<StrengthPower>()
        ];

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.AttackDashLight;
        public JumpTheGun(
            ) : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx(VfxCmd.slashPath)
                .Execute(choiceContext);
            await PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade() {
            DynamicVars.Strength.UpgradeValueBy(1);
        }

        public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
        {
            if (combatState.RoundNumber<=1&&side==Owner.Creature.Side&&Pile is { Type: PileType.Draw or PileType.Hand or PileType.Discard })
            {
                await CardCmd.AutoPlay(choiceContext, this,null);
            }
        }
    }
}
