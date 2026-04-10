using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Interface;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class MugenNoKosokuKiganPower : AbstractPower,IPersistCardSubscriber
    {
        public static readonly string ID = nameof(MugenNoKosokuKiganPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public async Task OnStartPersistCardLater(AbstractPersistCardSlot slot)
        {
            if (slot.Card.Owner.Creature!=Owner)return;
            if (!slot.Card.HasCounter())return;
            for (var i = 0; i < Amount; i++)
            {
                if (slot.Table==null)break;
                List<ICounter> counters = slot.Card.GetCountersForCard().ToList();
                if (counters.Count==0)break;
                await CounterCmd.InvokeCounter(CombatState, counters,
                    CombatState.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies));
            }
        }
    }
}