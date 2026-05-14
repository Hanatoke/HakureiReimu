using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Node.VFX.Special;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Common {
    public class DoubleBoundary : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new CounterVar(2),new BlockVar(7,ValueProp.Move)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Attack,Immediate];
        public override bool GainsBlock => true;
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellLongA;
        public DoubleBoundary(
            ) : base(2, CardType.Skill, CardRarity.Common, TargetType.Self) {
        }
        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            return Task.CompletedTask;
        }

        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            DynamicVars.Block.UpgradeValueBy(3);
        }
        public override bool IsImmediate => true;
        public override CounterType ActivateType => CounterType.Attack;
        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            await Flash(instant);
            Vfx(Owner.Creature, target);
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
            if (cost)
            {
                await Decrement();
            }
        }

        public void Vfx(Creature source, Creature target)
        {
            NCreature s = source?.GetCreatureNode();
            NCreature t = target?.GetCreatureNode();
            Control container = source?.GetVfxContainer();
            if (s == null||container==null)return;
            Vector2 sourcePos=s.VfxSpawnPosition;
            float scale = s.Body.GlobalScale.X*0.75f;
            Vector2 direction;
            if (t!=null)
            {
                direction=t.VfxSpawnPosition-sourcePos;
            }
            else
            {
                direction = s.Body.GlobalScale.X > 0 ? Vector2.Right : Vector2.Left;
            }
            FlyingVFXCmd.AddVFXOnTarget(NDoubleBarrier.Create(scale, direction.Normalized()),
                sourcePos + direction.Normalized() * 100f * scale, container);
        }
    }
}
