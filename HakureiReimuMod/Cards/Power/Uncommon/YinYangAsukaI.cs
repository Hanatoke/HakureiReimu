using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Uncommon {
    public class YinYangAsukaI : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<YinYangAsukaIPower>(1)
        ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromOrb<YinYangOrb>(),
            new HoverTip(TitleLocString, TipLocString)
        ];
        public YinYangAsukaI(
            ) : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<YinYangAsukaIPower>(choiceContext, Owner.Creature,
                DynamicVars[YinYangAsukaIPower.ID].BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade() 
        {
            DynamicVars[YinYangAsukaIPower.ID].UpgradeValueBy(1);
        }
        private LocString _tipLocString;
        public LocString TipLocString
        {
            get
            {
                _tipLocString ??= new LocString("cards", this.Id.Entry + ".tip");
                StringBuilder sb = new();
                bool newLine = true;
                foreach (PowerModel p in YinYangAsukaIPower.RandomPower)
                {
                    sb.Append(newLine ? "\n" : "    ");
                    newLine=!newLine;
                    sb.Append($"[gold]{p.Title.GetFormattedText()}[/gold]");
                }
                _tipLocString.Add("Desc",sb.ToString());
                return _tipLocString;
            }
        }
    }
}
