using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public class CounterVar :DynamicVar
    {
        public const string DefaultName ="Counter";
        public CounterVar(decimal baseValue) : base(DefaultName, baseValue)
        {
            this.WithTooltip(typeof(CounterVar).GetPrefix()+StringHelper.Slugify("Counter"),"card_keywords");
        }
    }
}