using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Hooks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class DreamInnate : AbstractCounterCard,IHealAmountModifier {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [All,Immediate,CardKeyword.Exhaust];

        public DreamInnate(
            ) : base(3, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }
        public override int Duration => 0;

        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
        public override bool IsImmediate => true;
        public override CounterType ActivateType => CounterType.All;
        
        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side==Owner.Creature.Side&&InPersisting&&Slot!=null)
            {
                await PersistCardCmd.StopPersistCard(Slot);
            }
        }
        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            RunAnimation(Character.HakureiReimu.Animation.Guard);
            await Flash(instant);
        }
        public override int ModifyAttackHitCount(AttackCommand attack, int hitCount)
        {
            if (InPersisting&&CheckAttack(attack))
            {
                return -999999999;
            }
            return hitCount;
        }
        public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel cardSource,
            CardPlay cardPlay)
        {
            if (InPersisting&&target is {IsMonster:true})
            {
                return 0;
            }
            return 1;
        }
        // public override decimal ModifyHealAmount(Creature creature, decimal amount)
        // {
        //     if (InPersisting&&creature is {IsMonster:true})
        //     {
        //         return creature.IsDead ? 1 : 0;
        //     }
        //     return amount;
        // }
        public decimal ModifyHealMultiplicative(Creature creature, decimal amount)
        {
            if (InPersisting && creature is { IsMonster: true })
            {
                return creature.IsDead ? 1/amount : 0;
            }
            return 1;
        }

        public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature applier,
            out decimal modifiedAmount)
        {
            if (InPersisting&&CheckPower(canonicalPower,amount,applier,target,out CounterType _)&&!PowerHelper.DontBlock.Contains(canonicalPower.GetType()))
            {
                modifiedAmount = 0;
                return true;
            }
            modifiedAmount = amount;
            return false;
        }

        public override async Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
        {
            if (InPersisting&&!addedByPlayer&&card.Type is CardType.Curse or CardType.Status)
            {
                if (CounterManager.InMonsterMove)
                {
                    CounterManager.AddToLater(this,async  () => await CounterCmd.InvokeCounter(CombatState,this,null));
                }
                else
                {
                    await CounterCmd.InvokeCounter(CombatState, this, null);
                }
                await CardPileCmd.RemoveFromCombat(card);
            }
        }

        // public static bool ShouldCancelMonsterAction(CombatState state)
        // {
        //     return state.IterateHookListeners().OfType<DreamInnate>().Any(d=>d.InPersisting);
        // }
    }
}
