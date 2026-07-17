using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class RetainBlockPower : AbstractPower
    {
        public static readonly string ID = nameof(RetainBlockPower);
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool ShouldClearBlock(Creature creature) => creature != this.Owner;
        public override async Task AfterPreventingBlockClear(AbstractModel preventer, Creature creature)
        {
            if (this!=preventer||creature!=Owner)
            {
                return;
            }
            int block=creature.Block;
            if (block==0)return;
            if (block>Amount)
            {
                await CreatureCmd.LoseBlock(new BlockingPlayerChoiceContext(),creature, block - Amount,null);
                await PowerCmd.Remove(this);
            }
        }
        public override async Task AfterSideTurnStart(CombatSide side,IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (participants.Contains(Owner))
            {
                await PowerCmd.Remove(this);
            }
        }
    }
}