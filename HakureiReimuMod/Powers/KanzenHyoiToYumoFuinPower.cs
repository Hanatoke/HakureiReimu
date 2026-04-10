using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using Godot;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class KanzenHyoiToYumoFuinPower : AbstractPower,IYinYangOrbListener
    {
        public static readonly int Div = 7;
        public static readonly string ID = nameof(KanzenHyoiToYumoFuinPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new DynamicVar("Div",Div),
            new DynamicVar("Total",0),
        ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<SealPower>()
        ];

        public int Calculate
        {
            get
            {
                int total = 0;
                foreach (Creature e in CombatState.HittableEnemies)
                {
                    total += e.GetPowerAmount<SealPower>();
                }
                return Mathf.FloorToInt((float)total/Div)*Amount;
            }
        }

        public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature applier, CardModel cardSource)
        {
            if (power is SealPower)
            {
                DynamicVars["Total"].BaseValue = Calculate;
            }
            return Task.CompletedTask;
        }

        public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (creature.HasPower<SealPower>())
            {
                DynamicVars["Total"].BaseValue = Calculate;
            }
            return Task.CompletedTask;
        }

        public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature dealer,
            CardModel cardSource)
        {
            return this.Owner != dealer || !props.IsPoweredAttack_()
                ? 0M
                : DynamicVars["Total"].BaseValue;
        }

        public decimal ModifyEvokeVal(YinYangOrb orb, decimal result)
        {
            return orb.Owner.Creature != Owner ? result : result+DynamicVars["Total"].BaseValue;
        }
    }
}