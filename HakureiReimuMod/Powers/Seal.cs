using System;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class Seal:AbstractPower
    {
        public static readonly string ID = nameof(Seal);
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public void ModifyDamage(ref decimal amount,ref ValueProp props, Creature dealer, CardModel cardSource)
        {
            decimal n=Math.Min(amount, this.Amount);
            amount =Math.Max(0,amount-n);
            Flash();
            PowerCmd.ModifyAmount(this, -n, null,null);
        }
    }
}