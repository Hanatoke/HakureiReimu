using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class UnconsciousTeleportation : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new CounterVar(1),new BlockVar(6,ValueProp.Move)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Attack,Immediate];
        public override bool GainsBlock => true;
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongA;
        public UnconsciousTeleportation(
            ) : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }
        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            DynamicVars[CounterVar.DefaultName].UpgradeValueBy(1);
        }
        public override bool IsImmediate => true;
        public override CounterType ActivateType => CounterType.Attack;
        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            await Flash(instant);
            CardPlay dummyPlay = new()
            {
                Card =  this,IsAutoPlay = false,PlayCount = 1,PlayIndex = 1,Resources = new ResourceInfo
                {
                    EnergySpent = 0,EnergyValue = 0,StarValue = 0,StarsSpent = 0
                },ResultPile = TargetPersistPileType,Target = Owner.Creature
            };
            decimal amount = Hook.ModifyBlock(CombatState, Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move,
                this, dummyPlay, out _);
            await CreatureCmd.GainBlock(Owner.Creature,amount,ValueProp.Unpowered,null,true);
            await PowerCmd.Apply<BlurPower>(Owner.Creature, 1,Owner.Creature,this);
            if (cost)
            {
                await Decrement();
            }
        }
    }
}
