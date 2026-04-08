using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Interface.Counter
{
    public interface ICounter
    {
        Creature CounterOwner => (this as CardModel)?.Owner.Creature;
        Task Invoke(Creature target, bool cost=true,bool instant=false);
    }
}