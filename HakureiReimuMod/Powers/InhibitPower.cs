using System;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class InhibitPower : AbstractPower
    {
        public static readonly string ID = nameof(InhibitPower);

        public override PowerType Type => PowerType.Debuff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool TryModifyPowerAmountReceived(
            PowerModel canonicalPower,
            Creature target,
            decimal amount,
            Creature _,
            out decimal modifiedAmount)
        {
            if (canonicalPower != this && target == Owner && amount>0 &&
                canonicalPower.GetTypeForAmount(amount) == PowerType.Buff && canonicalPower.IsVisible &&
                !PowerHelper.DontBlock.Contains(canonicalPower.GetType()))
            {
                modifiedAmount = 0;
                return true;
            }
            modifiedAmount = amount;
            return false;
        }
        public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
        {
            Flash();
            await PowerCmd.Decrement(this);
        }
    }
}