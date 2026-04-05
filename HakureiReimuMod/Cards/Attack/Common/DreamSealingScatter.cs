using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class DreamSealingScatter : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(14,ValueProp.Move),new PowerVar<VulnerablePower>(2)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

        public DreamSealingScatter(
            ) : base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            // SfxCmd.Play("event:/sfx/characters/silent/silent_dagger_spray");
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this).TargetingAllOpponents(CombatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            foreach (Creature e in CombatState.HittableEnemies)
            {
                if (e.HasPower<Powers.Seal>())
                {
                    await PowerCmd.Apply<VulnerablePower>(e, DynamicVars.Vulnerable.IntValue, Owner.Creature, this);
                }
            }
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(4);
            DynamicVars.Vulnerable.UpgradeValueBy(1);
        }
    }
}
