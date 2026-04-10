using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class RanboNaSekaiNoSakemeguchi : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(Counter),
            HoverTipFactory.FromKeyword(FreeCounter),
        ];
        protected override bool HasEnergyCostX => true;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new PowerVar<Powers.SealPower>(6),new PowerVar<WeakPower>(1)];
        
        public RanboNaSekaiNoSakemeguchi(
            ) : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1)
            {
                Cancelable = false
            };
            CardModel c=(await CardSelectCmd.FromSimpleGrid(choiceContext, Owner.GetAllCounterCards(), Owner, prefs)).FirstOrDefault();
            if (c!=null)
            {
                int x = ResolveEnergyXValue();
                if (IsUpgraded) x++;
                for (var i = 0; i < x; i++)
                {
                    foreach (ICounter counter in c.GetCountersForCard())
                    {
                        await CounterCmd.InvokeCounter(c.CombatState, counter,cardPlay.Target,false);
                    }
                }
            }
        }
    }
}
