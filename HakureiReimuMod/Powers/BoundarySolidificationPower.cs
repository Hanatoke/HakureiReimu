using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using HakureiReimu.HakureiReimuMod.PersistCard.Interface;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class BoundarySolidificationPower : AbstractPower,IPersistCardSubscriber
    {
        public static readonly string ID = nameof(BoundarySolidificationPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public async Task OnStartPersistCard(AbstractPersistCardSlot slot)
        {
            if (slot.Card!=null&&slot.Card.Owner.Creature==Owner&&slot.Card.HasCounter())
            {
                Flash();
                await PersistCardCmd.IncreaseCount(slot, Amount);
            }
        }
    }
}