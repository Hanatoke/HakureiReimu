using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class SealPower:AbstractPower
    {
        public static readonly string ID = nameof(SealPower);
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public int NeedToCost=0;
        // public void ModifyDamages(ref decimal amount,ref ValueProp props, Creature dealer, CardModel cardSource, IEnumerable<Creature> targets)
        // {
        //     decimal total = 0;
        //     foreach (Creature player in targets.ToList())
        //     {
        //         decimal d=Hook.ModifyDamage(CombatState.RunState, CombatState, player, dealer, amount, props, cardSource,
        //             ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);
        //         total = Math.Max(total, d);
        //     }
        //     if (total<=0)return;
        //     
        //     decimal n=Math.Min(total, this.Amount);
        //     // amount = Math.Max(0, Math.Floor(amount * (1 - n / total)));
        //     amount = amount-n;
        //     Flash();
        //     PowerCmd.ModifyAmount(this, -n, null,null);
        //     foreach (Creature p in targets.ToList())
        //     {
        //         if (p.HasPower<KujiKoshinHoPower>())
        //         {
        //             if (Owner.Monster is {IsPerformingMove:true})
        //             {
        //                 KujiKoshinHoPower power = p.GetPower<KujiKoshinHoPower>();
        //                 power?.TryAddToLater(Owner,n);
        //             }
        //             else
        //             {
        //                 PowerCmd.Apply<SealPower>(Owner,n, Owner, null);
        //             }
        //             return;
        //         }
        //     }
        // }

        public void ModifyDamage(ref decimal amount, ref ValueProp props, Creature dealer, CardModel cardSource,
            Creature target)
        {
            if (amount>0)
            {
                decimal n=Math.Min(amount, this.Amount);
                amount=Math.Max(0,amount-n);
                NeedToCost = Math.Max(NeedToCost, (int)n);
            }
        }

        public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props,
            Creature target, CardModel cardSource)
        {
            if (dealer==Owner&&NeedToCost>0&&Owner.IsAlive)
            {
                int n = NeedToCost;
                NeedToCost = 0;
                Flash();
                await PowerCmd.ModifyAmount(this,-n,null,null);
                foreach (Creature p in CombatState.GetOpponentsOf(Owner).ToList())
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
                            await PowerCmd.Apply<SealPower>(Owner,n, Owner, null);
                        }
                        return;
                    }
                }
            }
        }
    }
}