using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class DreamSealingInstant : AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DamageVar(12, ValueProp.Move)
            ];

        public DreamSealingInstant(
            ) : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .BeforeDamage(async ()=>await FlyingVFXCmd.DanmakuLineToTarget(Owner.Creature,cardPlay.Target))
                .Execute(choiceContext);
            await CardPileCmd.Add(Owner.PlayerCombatState.DrawPile.Cards.Where(c => c.Type == CardType.Attack).ToList(),
                PileType.Hand, source: this);
        }

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(4);
        }
    }
}
