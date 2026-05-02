using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class HappoRyuSatsuJin : AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DamageVar(8, ValueProp.Move)
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromKeyword(Counter),
        ];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.AttackCloseRound;
        public HappoRyuSatsuJin(
            ) : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            foreach (AbstractModel abstractModel in CombatState.IterateHookListeners())
            {
                if (abstractModel is ICounter{IsCounterEnable:true} counter && counter.CounterOwner==Owner.Creature)
                {
                    await CounterCmd.InvokeCounter(CombatState, counter, cardPlay.Target, true, true);
                }
            }
        }

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(4);
        }
    }
}
