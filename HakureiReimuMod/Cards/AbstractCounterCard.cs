#nullable enable
using System;
using System.Threading.Tasks;
using BaseLib.Extensions;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards
{
    public abstract class AbstractCounterCard(int cost, CardType type, CardRarity rarity, TargetType target) 
        :AbstractPersistCard(cost, type, rarity, target),ICounter
    {
        public Creature CounterOwner => this.Owner.Creature;
        public bool IsCounterEnable => InPersisting;
        public virtual bool IsImmediate=>false;
        public virtual CounterType ActivateType => CounterType.None;

        protected virtual bool CheckAttack(AttackCommand command) => ActivateType.HasFlag(CounterType.Attack)&& command.Attacker is { IsMonster: true } &&
                                                                     command.DamageProps.IsCardOrMonsterMove_();

        protected virtual bool CheckPower(PowerModel power, decimal modifiedAmount, Creature applier, Creature target,out CounterType counterType)
        {
            PowerType type=power.GetTypeForAmount(modifiedAmount);
            if (power.IsVisible&&type == PowerType.Buff&&modifiedAmount>0&& ActivateType.HasFlag(CounterType.Buff)&&applier is { IsMonster: true }&&target is{IsMonster:true})
            {
                counterType = CounterType.Buff;
                return true;
            }
            if (power.IsVisible&&type == PowerType.Debuff&&ActivateType.HasFlag(CounterType.Debuff)&&applier is { IsMonster: true }&&target is{IsPlayer:true})
            {
                counterType = CounterType.Debuff;
                return true;
            }
            counterType = CounterType.None;
            return false;
        }
        //-------------------------------------------------------------------------------------------------------
        public override async Task BeforeAttack(AttackCommand command)
        {
            if (InPersisting&&IsImmediate&&CheckAttack(command))
            {
                await CounterManager.ForceAttackAnimation(command);
                await InvokeCounter(command.Attacker,CounterType.Attack);
            }
        }

        public override async Task AfterAttack(AttackCommand command)
        {
            if (InPersisting&&!IsImmediate&&CheckAttack(command))
            {
                await InvokeCounter(command.Attacker,CounterType.Attack);
            }
        }
        //------------------------------------------------------------------------------------------------------
        public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel cardSource)
        {
            if (InPersisting&&ActivateType.HasFlag(CounterType.Buff)&&creature is {IsMonster:true})
            {
                await InvokeCounter(creature,CounterType.Buff);
            }
        }

        public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
        {
            if (InPersisting&&ActivateType.HasFlag(CounterType.Buff)&&delta>0&&creature is {IsMonster:true})
            {
                await InvokeCounter(creature,CounterType.Buff);
            }
        }
        public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature applier, CardModel cardSource)
        {
            if (InPersisting&&CheckPower(power,amount,applier,power.Owner,out CounterType t))
            {
                await InvokeCounter(applier,t);
            }
        }

        public override async Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
        {
            if (InPersisting&&ActivateType.HasFlag(CounterType.Debuff)&&!addedByPlayer&&card.Type is CardType.Curse or CardType.Status)
            {
                await InvokeCounter(null,CounterType.Debuff);
            }
        }

        public virtual async Task InvokeCounter(Creature? target,CounterType byType)
        {
            if (!IsImmediate&&CounterManager.InMonsterMove)
            {
                CounterManager.AddToLater(this,async  () => await CounterCmd.InvokeCounter(CombatState,this,target));
            }
            else
            {
                await CounterCmd.InvokeCounter(CombatState, this, target);
            }
        }

        [Flags]
        public enum CounterType
        {
            None=0,
            Attack=1<<1,
            Buff=1<<2,
            Debuff=1<<3,
            All=Attack|Buff|Debuff,
        }
        public abstract Task Invoke(Creature? target, bool cost = true, bool instant = false);
    }
}