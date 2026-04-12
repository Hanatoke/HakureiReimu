using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class Shoryuken : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [new CounterVar(3),new DamageVar(4,ValueProp.Move),new PowerVar<VulnerablePower>(1)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Buff];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellFastA;
        public Shoryuken(
            ) : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies) {
        }

        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }

        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(1);
            DynamicVars.Vulnerable.UpgradeValueBy(1);
        }
        public override CounterType ActivateType => CounterType.Buff;
        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            if (target is not { IsHittable: true }) return;
            RunAnimation(Character.HakureiReimu.Animation.DamageLight);
            await Flash(instant);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(null);
            await PowerCmd.Apply<VulnerablePower>(target, DynamicVars.Vulnerable.IntValue, Owner.Creature, this);
            if (cost)
            {
                await Decrement();
            }
        }
    }
}
