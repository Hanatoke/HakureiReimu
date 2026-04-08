using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using HakureiReimu.HakureiReimuMod.PersistCard.Interface;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class YinYangGyokushoPower : AbstractPower,IYinYangOrbListener
    {
        public static readonly string ID = nameof(YinYangGyokushoPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public decimal ModifyEvokeVal(YinYangOrb orb, decimal result)
        {
            if (orb?.Owner.Creature==Owner)
            {
                return  result+Amount;
            }
            return result;
        }
    }
}