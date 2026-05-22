using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class KujiKoshinHoPower : AbstractPower
    {
        public static readonly string ID = nameof(KujiKoshinHoPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public Dictionary<Creature, decimal> NeedAddToLater = new();
        public override async Task AfterSideTurnStart(CombatSide side,IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (participants.Contains(Owner))
            {
                await PowerCmd.TickDownDuration(this);
            }
        }

        public void TryAddToLater(Creature creature, decimal amount)
        {
            if (!NeedAddToLater.TryAdd(creature, amount))
            {
                NeedAddToLater[creature] += amount;
            }
        }

        public override async Task AfterAttack(PlayerChoiceContext context,AttackCommand command)
        {
            foreach (var keyValuePair in NeedAddToLater)
            {
                Flash();
                await PowerCmd.Apply<SealPower>(new BlockingPlayerChoiceContext(), keyValuePair.Key, keyValuePair.Value,keyValuePair.Key,null);
            }
            NeedAddToLater.Clear();
        }
    }
}