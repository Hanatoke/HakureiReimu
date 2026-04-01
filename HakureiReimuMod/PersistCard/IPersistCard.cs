using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.PersistCard
{
    public interface IPersistCard
    {
        CardModel Model { get; }
        AbstractPersistCardSlot InstanceSlot{get;}
    }
}