using System;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Patches;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public class ReimuGlobalModel : SingletonModel
    {
        public override bool ShouldReceiveCombatHooks => true;

        public override decimal ModifyPowerAmountGiven(PowerModel power, Creature giver, decimal amount, Creature target,
            CardModel cardSource)
        {
            if (power is SealPower && amount > 0 && giver is { Side: CombatSide.Player, CombatState: { } state })
            {
                if (target is {Side:CombatSide.Player}) return amount;
                int count = state.RunState.Players.Count(p => p.Creature.IsAlive);
                if (count<=1)return amount;
                decimal d = amount * SealPower.MultiplayerScaling(count);
                return d > 1 ? Math.Floor(d) : Math.Ceiling(d);
            }
            return amount;
        }

        public override Task BeforeAttack(AttackCommand command)
        {
            if (command.ModelSource is CardModel card && card.Keywords.Contains(AbstractCard.IgnoreDefense))
            {
                command.AddDamageProps(ValueProp.Unblockable|DamagePropsPatch.IgnoreDamageImmunity|DamagePropsPatch.IgnoreDamageResponse);
            }
            return Task.CompletedTask;
        }
    }
}