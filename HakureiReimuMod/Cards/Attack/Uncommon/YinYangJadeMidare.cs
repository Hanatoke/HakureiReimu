using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class YinYangJadeMidare : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
                new DamageVar(3,ValueProp.Move),
                new RepeatVar(1),
                new CalculationBaseVar(0),
                new CalculationExtraVar(1),
                new CalculatedVar("CalculatedTimes").WithMultiplier((c,_)=>c.Owner.PlayerCombatState?.YinYangOrbManager()?.Orbs.Count??0)
            ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>()];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.ShotA;
        public YinYangJadeMidare(
            ) : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int n = (int)((CalculatedVar)DynamicVars["CalculatedTimes"]).Calculate(cardPlay.Target);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).WithHitCount(n)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);
            await YinYangOrbCmd.Spawn(choiceContext,Owner,DynamicVars.Repeat.IntValue*n, this);
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
