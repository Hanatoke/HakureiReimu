using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class DoragonzuAsukaiKikku : AbstractCard
    {

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new CalculationBaseVar(30),
                new ExtraDamageVar(3),
                new CalculatedDamageVar(ValueProp.Move).WithMultiplier(
                    (c,_)=>
                    {
                        int count = PileType.Hand.GetPile(c.Owner).Cards.Count;
                        if (c.Pile is { Type: PileType.Hand }) count--;
                        return -count;
                    })
            ];
        
        public DoragonzuAsukaiKikku(
            ) : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(DynamicVars.CalculatedDamage.Calculate(cardPlay.Target)).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
                .Execute(choiceContext);
        }
        protected override void OnUpgrade() {
            DynamicVars.CalculationBase.UpgradeValueBy(10);
        }
    }
}
