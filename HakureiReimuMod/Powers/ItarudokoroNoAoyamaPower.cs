using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Interface;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class ItarudokoroNoAoyamaPower : AbstractPower,IPersistCardSubscriber
    {
        public static readonly string ID = nameof(ItarudokoroNoAoyamaPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public async Task OnStartPersistCard(AbstractPersistCardSlot slot)
        {
            if (slot.Card!=null&&slot.Card.Owner.Creature==Owner&&slot.Card.HasCounter())
            {
                Flash();
                await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), Amount, slot.Card.Owner);
            }
        }
    }
}