using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class HakureiGreatBoundary : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new CounterVar(1)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Buff,CardKeyword.Exhaust];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongA;
        public HakureiGreatBoundary(
            ) : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }
        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            DynamicVars[CounterVar.DefaultName].UpgradeValueBy(1);
        }
        public override bool IsImmediate => true;
        public override CounterType ActivateType => CounterType.Buff;
        public override Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            return Task.CompletedTask;
            // await Flash(instant);
            //
            // if (cost)
            // {
            //     await Decrement();
            // }
        }

        public async Task Decrease()
        {
            await Flash();
            await Decrement();
        }

        public override async Task InvokeCounter(Creature target, CounterType byType)
        {
            if (!IsInCombat)
            {
                MainFile.Logger.Warn("尝试发动不在战斗中的反制卡? "+this.GetType().Name);
                return;
            }
            if (!IsImmediate&&CounterManager.InMonsterMove)
            {
                CounterManager.AddToLater(this,async  () =>
                {
                    await CounterCmd.InvokeCounter(CombatState, this, target);
                    await Decrease();
                });
            }
            else
            {
                await CounterCmd.InvokeCounter(CombatState, this, target);
                await Decrease();
            }
        }

        public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel cardSource)
        {
            if (InPersisting&&ActivateType.HasFlag(CounterType.Buff)&&amount>0&&creature is {IsMonster:true})
            {
                await Flash(true);
                await CreatureCmd.GainBlock(Owner.Creature, amount, ValueProp.Unpowered, null);
                if (Owner.Creature.Side!=Owner.Creature.CombatState.CurrentSide)
                {
                    await PowerCmd.Apply<RetainBlockPower>(Owner.Creature, amount, Owner.Creature, this);
                }
                await InvokeCounter(creature,CounterType.Buff);
            }
        }
        public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
        {
            if (InPersisting&&ActivateType.HasFlag(CounterType.Buff)&&delta>0&&creature is {IsMonster:true})
            {
                await Flash(true);
                await CreatureCmd.Heal(Owner.Creature,delta);
                await InvokeCounter(creature,CounterType.Buff);
            }
        }
        public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature applier, CardModel cardSource)
        {
            if (InPersisting&&CheckPower(power,amount,applier,power.Owner,out CounterType t)&&amount>0)
            {
                await Flash(true);
                if (PlayerUsefulPowers.Contains(power.GetType()))
                {
                    await PowerCmd.Apply((PowerModel)power.ClonePreservingMutability(),Owner.Creature, amount, Owner.Creature, this);
                }
                else
                {
                    await PowerCmd.Apply<StrengthPower>(Owner.Creature, 1, Owner.Creature, this);
                }
                await InvokeCounter(applier,t);
            }
        }
        public static readonly HashSet<Type> PlayerUsefulPowers = [
            typeof(ArtifactPower),
            typeof(BarricadePower),
            typeof(BufferPower),
            typeof(CurlUpPower),
            typeof(EnragePower),
            typeof(HighVoltagePower),
            typeof(IntangiblePower),
            typeof(PlatingPower),
            typeof(RegenPower),
            typeof(RitualPower),
            typeof(ThornsPower),
            typeof(VigorPower),
            typeof(StrengthPower),
            typeof(DexterityPower),
        ];
    }
}
