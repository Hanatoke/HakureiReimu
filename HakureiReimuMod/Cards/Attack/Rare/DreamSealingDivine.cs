using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class DreamSealingDivine : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.SealPower>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DamageVar(6, ValueProp.Move),
                new PowerVar<Powers.SealPower>(6),
                new RepeatVar(3)
            ];
        
        public DreamSealingDivine(
            ) : base(0, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            NCreature s = NCombatRoom.Instance?.GetCreatureNode(Owner.Creature);
            NCreature t = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
            if (s != null && t != null)
            {
                List<Task> tasks = [];
                int num = 8;
                for (var i = 0; i < num; i++)
                {
                    var index = i;
                    Color c = Color.FromHsv(GD.Randf(), 1, 1);
                    Vector2 v = Vector2.Left.Rotated(Mathf.DegToRad(Mathf.Lerp(180, 0, (float)i / num))) * 500f;
                    FlyingVFX vfx=FlyingVFX.Create(
                        new SteeringMover(s.VfxSpawnPosition,
                            t.VfxSpawnPosition+FlyingVFXCmd.RandomOffset(4),
                            v,0,500));
                    vfx.Duration = 3;
                    vfx.UpdateMethod = (time, _) =>
                    {
                        if (time>0.2f)
                        {
                            SteeringMover mover = (SteeringMover)vfx.Mover;
                            mover.TurnSpeed = 10+index;
                            mover.Acceleration = 2000+index*100;
                            mover.MaxSpeed += mover.Acceleration*2;
                        }
                        vfx.Scale = Mathf.Lerp(2, 6, time) * Vector2.One;
                    };
                    vfx.OnHit = () =>
                    {
                        NDebugAudioManager.Instance?.Play("blunt_attack.mp3");
                        FlyingVFXCmd.AddVFXOnTarget(NDanmakuImpact.Create(4,c),vfx.GlobalPosition);
                    };
                    vfx.AddChildSafely(NDanmaku.Create(color:c));
                    NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    tasks.Add(vfx.HitTask);
                    await Cmd.Wait(0.1f);
                }
                await Task.WhenAny(tasks);
            }
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).WithHitCount(DynamicVars.Repeat.IntValue)
                .Execute(choiceContext);
        }

        public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props,
            Creature target, CardModel cardSource)
        {
            if (cardSource==this)
            {
                await PowerCmd.Apply<SealPower>(target, DynamicVars[SealPower.ID].BaseValue, Owner.Creature, this);
            }
        }

        protected override void OnUpgrade() {
            DynamicVars.Repeat.UpgradeValueBy(1);
        }
    }
}
