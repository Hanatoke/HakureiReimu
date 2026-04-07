using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Interface.Counter.Hook
{
    public interface ICounterListener
    {
        Task BeforeCounter(ICounter counter);
        Task AfterCounter(ICounter counter);
    }
}