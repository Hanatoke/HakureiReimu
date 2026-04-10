using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class CheatBoundary : AbstractPersistCard
    {
        private static LocString _exTip;
        public LocString ExTip => _exTip ??= LocString.GetIfExists("cards", this.Id.Entry + ".tip");
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(Counter),
            HoverTipFactory.FromKeyword(FreeCounter),
            new HoverTip(TitleLocString,ExTip)
        ];
        public CheatBoundary(
            ) : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            return Task.CompletedTask;
        }
        protected override void OnUpgrade() {
            EnergyCost.UpgradeBy(-1);
        }
        public override int Duration => 0;
        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side==Owner.Creature.Side&&InPersisting&&Slot!=null)
            {
                await PersistCardCmd.StopPersistCard(Slot);
            }
        }

        public override async Task AfterCardPlayedLate(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (InPersisting&&cardPlay.Card.Owner==this.Owner&&this.Pile is AbstractPersistCardTable table)
            {
                Creature target = cardPlay.Target is { IsEnemy: true }
                    ? cardPlay.Target
                    : CombatState.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
                int index = table.Cards.IndexOf(this);
                if (index < 0)
                {
                    MainFile.Logger.Warn("出现了超出预期流程的情况:"+this.Title+":"+cardPlay.Card.Title);
                    return;
                }
                CardModel left = index - 1 >= 0 ? table.Cards[index - 1] : null;
                CardModel right = index + 1 < table.Cards.Count ? table.Cards[index + 1] : null;
                List<ICounter> countersForCard;
                if (left!=null)
                {
                    countersForCard = left.GetCountersForCard().ToList();
                    if (countersForCard.Count>0)
                    {
                        await Flash(true);
                    }
                    await CounterCmd.InvokeCounter(CombatState, countersForCard, target,
                        false);
                }
                if (right!=null)
                {
                    countersForCard = right.GetCountersForCard().ToList();
                    if (countersForCard.Count>0)
                    {
                        await Flash(true);
                    }
                    await CounterCmd.InvokeCounter(CombatState, countersForCard, target,
                        false);
                }
            }
        }
    }
}
