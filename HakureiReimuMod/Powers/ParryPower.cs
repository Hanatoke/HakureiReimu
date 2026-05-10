using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using HakureiReimu.HakureiReimuMod.Interface.Counter.Hook;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class ParryPower : AbstractPower,ICounterListener
    {
        public static readonly string ID = nameof(ParryPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public async Task AfterCounter(ICombatState state, ICounter counter, Creature target)
        {
            if (counter?.CounterOwner==Owner)
            {
                Flash();
                await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, true);
            }
        }
    }
}