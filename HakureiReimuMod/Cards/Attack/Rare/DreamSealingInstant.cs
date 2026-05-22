using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class DreamSealingInstant : AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DamageVar(12, ValueProp.Move)
            ];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.AttackCloseHeavy;
        public DreamSealingInstant(
            ) : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                // .BeforeDamage(async ()=>await FlyingVFXCmd.DanmakuLineToTarget(Owner.Creature,cardPlay.Target))
                .WithHitVfxNode(NGrandFinaleImpactVfx.Create)
                .WithHitFx(tmpSfx: "blunt_attack.mp3")
                .Execute(choiceContext);
            
            List<CardModel> cards = Owner.PlayerCombatState.DrawPile.Cards.Where(c => c.Type == CardType.Attack).ToList();
            foreach (CardModel card in cards)
            {
                await CardPileCmd.Add(cards, PileType.Hand);
                await Hook.AfterCardDrawn(CombatState, choiceContext, card, false);
            }
        }

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(4);
        }
    }
}
