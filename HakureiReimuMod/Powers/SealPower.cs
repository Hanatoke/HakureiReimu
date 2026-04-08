using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class SealPower:AbstractPower
    {
        public static readonly string ID = nameof(SealPower);
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public void ModifyDamage(ref decimal amount,ref ValueProp props, Creature dealer, CardModel cardSource, IEnumerable<Creature> targets)
        {
            decimal n=Math.Min(amount, this.Amount);
            amount =Math.Max(0,amount-n);
            Flash();
            PowerCmd.ModifyAmount(this, -n, null,null);
            
            foreach (Creature p in targets.ToList())
            {
                if (p.HasPower<KujiKoshinHoPower>())
                {
                    if (Owner.Monster is {IsPerformingMove:true})
                    {
                        KujiKoshinHoPower power = p.GetPower<KujiKoshinHoPower>();
                        power?.TryAddToLater(Owner,n);
                    }
                    else
                    {
                        PowerCmd.Apply<SealPower>(Owner,n, Owner, null);
                    }
                    return;
                }
            }
        }
    }
}