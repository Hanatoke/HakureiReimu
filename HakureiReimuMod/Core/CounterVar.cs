using BaseLib.Extensions;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public class CounterVar :DynamicVar
    {
        public const string DefaultName ="Counter";
        public CounterVar(decimal baseValue) : base(DefaultName, baseValue)
        {
            this.WithTooltip(typeof(CounterVar).GetPrefix()+StringHelper.Slugify("Counter"),"card_keywords");
        }

        public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature target, bool runGlobalHooks)
        {
            decimal v = BaseValue;
            if (runGlobalHooks)
            {
                v += card.Owner.Creature.GetPowerAmount<BoundarySolidificationPower>();
            }
            PreviewValue = v;
        }
    }
}