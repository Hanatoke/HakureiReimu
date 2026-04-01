using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HakureiReimu.HakureiReimuMod.Interface.Counter
{
    public interface ICounter
    {
        Task Invoke(Creature target, bool cost=true,bool instant=false);
    }
}