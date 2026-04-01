using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public class CounterVar :DynamicVar
    {
        public const string DefaultName = MainFile.ModId+"_Counter";
        public CounterVar(decimal baseValue) : base(DefaultName, baseValue)
        {
        }
    }
}