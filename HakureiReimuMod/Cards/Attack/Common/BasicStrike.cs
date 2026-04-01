using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class BasicStrike : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DamageVar>() {new(6,ValueProp.Move)};
        protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
        public BasicStrike(
            ) : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(3);
        }
    }
}
