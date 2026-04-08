using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class MashoSenketsu : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new CounterVar(2)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Debuff,CardKeyword.Exhaust];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [];

        public MashoSenketsu(
            ) : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }
        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            DynamicVars[CounterVar.DefaultName].UpgradeValueBy(1);
        }
        public override CounterType ActivateType => CounterType.Debuff;
        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            await Flash(instant);
            foreach (PowerModel p in Owner.Creature.Powers.ToList())
            {
                if (p.TypeForCurrentAmount==PowerType.Debuff&&p.IsVisible)
                {
                    await PowerCmd.Remove(p);
                    break;
                }
            }

            if (Owner.PlayerCombatState!=null)
            {
                if (await FindCard(Owner.PlayerCombatState.Hand.Cards)) { }
                else if (await FindCard(Owner.PlayerCombatState.DrawPile.Cards)) { }
                else if (await FindCard(Owner.PlayerCombatState.DiscardPile.Cards)) { }
            }
            if (cost)
            {
                await Decrement();
            }
        }

        protected async Task<bool> FindCard(IEnumerable<CardModel> cards)
        {
            foreach (CardModel c in cards.ToList())
            {
                if (c.Type==CardType.Status||c.Type==CardType.Curse)
                {
                    await CardCmd.Exhaust(new BlockingPlayerChoiceContext(), c);
                    return true;
                }
            }
            return false;
        }
    }
}
