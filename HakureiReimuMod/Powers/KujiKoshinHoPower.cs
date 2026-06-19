using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class KujiKoshinHoPower : AbstractPower
    {
        public static readonly string ID = nameof(KujiKoshinHoPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        // public Dictionary<Creature, decimal> NeedAddToLater = new();
        // public override async Task AfterSideTurnStart(CombatSide side,IReadOnlyList<Creature> participants, ICombatState combatState)
        // {
        //     if (participants.Contains(Owner))
        //     {
        //         await PowerCmd.TickDownDuration(this);
        //     }
        // }
        //
        // public void TryAddToLater(Creature creature, decimal amount)
        // {
        //     if (!NeedAddToLater.TryAdd(creature, amount))
        //     {
        //         NeedAddToLater[creature] += amount;
        //     }
        // }
        //
        // public override async Task AfterAttack(PlayerChoiceContext context,AttackCommand command)
        // {
        //     foreach (var keyValuePair in NeedAddToLater)
        //     {
        //         Flash();
        //         await PowerCmd.Apply<SealPower>(new BlockingPlayerChoiceContext(), keyValuePair.Key, keyValuePair.Value,keyValuePair.Key,null);
        //     }
        //     NeedAddToLater.Clear();
        // }
        public Dictionary<Creature, bool> Record = new();
        protected override void DeepCloneFields()
        {
            base.DeepCloneFields();
            Record = Record.ToDictionary();
        }

        public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature dealer,
            CardModel cardSource)
        {
            if (target == Owner && dealer != null && dealer != Owner && Verify(dealer))
            {
                return -Amount;
            }
            return 0;
        }

        public bool Verify(Creature owner)
        {
            bool isPerformingMove = owner.IsMonster && owner.Monster?.IsPerformingMove == true;
            if (owner.HasPower<SealPower>())
            {
                if (isPerformingMove)
                {
                    Record[owner] = true;
                }
                return true;
            }
            return isPerformingMove && Record.GetValueOrDefault(owner, false);
        }

        public override Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
        {
            Record.Clear();
            return Task.CompletedTask;
        }

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side != CombatSide.Enemy) return;
            await PowerCmd.Remove(this);
        }
    }
}