using System;
using System.Linq;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public class ReimuMultiplayerScalingModel : SingletonModel
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
    }
}