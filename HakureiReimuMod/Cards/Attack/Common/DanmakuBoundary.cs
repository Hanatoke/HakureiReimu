using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class DanmakuBoundary : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CounterVar(2),new DamageVar(6,ValueProp.Move)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Attack,Immediate,CardKeyword.Exhaust];

        public DanmakuBoundary(
            ) : base(1, CardType.Attack, CardRarity.Token, TargetType.AllEnemies) {
        }

        public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count,CombatState state)
        {
            if (count == 0) return [];
            if (CombatManager.Instance.IsOverOrEnding) return [];
            List<CardModel> cards = new();
            for (var i = 0; i < count; i++)
            {
                cards.Add(state.CreateCard<DanmakuBoundary>(owner));
            }
            await CardPileCmd.AddGeneratedCardsToCombat(cards,PileType.Hand,true);
            return cards;
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }

        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(3);
        }
        public override bool IsImmediate => true;
        public override CounterType ActivateType => CounterType.Attack;
        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            if (target is not { IsHittable: true }) return;
            await Flash(instant);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(null);
            if (cost)
            {
                await Decrement();
            }
        }
    }
}
