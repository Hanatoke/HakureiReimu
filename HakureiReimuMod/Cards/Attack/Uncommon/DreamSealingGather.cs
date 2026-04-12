using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class DreamSealingGather : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6,ValueProp.Move),new RepeatVar(3)];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.ShotA;
        public DreamSealingGather(
            ) : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            List<Task> waits = [];
            for (var i = 0; i < DynamicVars.Repeat.IntValue; i++)
            {
                waits.Add(FlyingVFXCmd.DanmakuCurveToTarget(Owner.Creature, cardPlay.Target, 3));
                await Cmd.Wait(0.25f);
            }
            await Task.WhenAny(waits);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars.Repeat.IntValue)
                .FromCard(this).Targeting(cardPlay.Target)
                .Execute(choiceContext);
            EnergyCost.AddThisCombat(-1);
        }
        protected override void OnUpgrade() {
            DynamicVars.Repeat.UpgradeValueBy(1);
        }
    }
}
