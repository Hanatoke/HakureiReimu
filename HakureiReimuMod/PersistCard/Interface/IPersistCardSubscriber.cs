using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.PersistCard.Interface
{
    public interface IPersistCardSubscriber
    {
        Task OnStartPersistCard(AbstractPersistCardSlot slot){return Task.CompletedTask;}
        Task OnStopPersistCard(AbstractPersistCardSlot slot){return Task.CompletedTask;}
        bool AtIncreasePersistCount(AbstractPersistCardSlot slot, int origin, ref int result) => true;
        bool AtDecreasePersistCount(AbstractPersistCardSlot slot, int origin, ref int result) => true;
        Task AfterModifyPersistCount(AbstractPersistCardSlot slot,int result){return Task.CompletedTask;}
    }
}