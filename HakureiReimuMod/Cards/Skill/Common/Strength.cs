using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class Strength : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new CounterVar(2),new CardsVar(5)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Debuff,CardKeyword.Exhaust];

        public Strength(
            ) : base(1, CardType.Skill, CardRarity.Common, TargetType.None) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }

        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            RemoveKeyword(Debuff);
            AddKeyword(All);
        }
        public override CounterType ActivateType => IsUpgraded ? CounterType.All : CounterType.Debuff;
        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            if (PileType.Draw.GetPile(Owner).Cards.Count<=0) return;
            await Flash(instant);
            await Effect();
            if (cost)
            {
                await Decrement();
            }
        }
        public async Task Effect()
        {
            List<CardModel> toUpgrade = new();
            int n = DynamicVars.Cards.IntValue;
            foreach (CardModel c in PileType.Draw.GetPile(Owner).Cards)
            {
                if (c.IsUpgradable)
                {
                    toUpgrade.Add(c);
                    n--;
                    if (n<=0)break;
                }
            }
            CardCmd.Upgrade(toUpgrade,CardPreviewStyle.HorizontalLayout);
            CardCmd.Preview(toUpgrade);
            await Cmd.Wait(toUpgrade.Count*0.1f);
        }
    }
}
