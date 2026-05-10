using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Interface.Counter.Hook
{
    public interface ICounterListener
    {
        Task BeforeCounter(ICombatState state,ICounter counter,Creature target){return Task.CompletedTask;}
        Task AfterCounter(ICombatState state,ICounter counter,Creature target){return Task.CompletedTask;}
    }
}