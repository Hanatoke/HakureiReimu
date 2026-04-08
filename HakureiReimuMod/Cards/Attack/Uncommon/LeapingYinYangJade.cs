using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class LeapingYinYangJade : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DamageVar(3,ValueProp.Move),
                new RepeatVar(3),
                new PowerVar<VulnerablePower>(1)
            ];

        public LeapingYinYangJade(
            ) : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            
            AttackCommand dummy = DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this)
                .TargetingRandomOpponents(CombatState);
            await Hook.BeforeAttack(CombatState,dummy);
            int repeat =(int) Hook.ModifyAttackHitCount(CombatState, dummy, DynamicVars.Repeat.IntValue);
            NCreature startPos = NCombatRoom.Instance?.GetCreatureNode(Owner.Creature);
            for (var i = 0; i < repeat; i++)
            {
                Creature target=CombatState.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
                if (target==null)break;
                NCreature targetPos = NCombatRoom.Instance?.GetCreatureNode(target);
                if (startPos!=null&&targetPos != null)
                {
                    FlyingVFX vfx = FlyingVFX.Create(
                        new SteeringMover(startPos.VfxSpawnPosition, targetPos.VfxSpawnPosition, Vector2.One * 1000,
                            turnSpeed: 720f));
                    Node2D orb = PreloadManager.Cache.GetScene(YinYangOrb.ScenePath).Instantiate<Node2D>();
                    vfx.AddChildSafely(orb);
                    vfx.Scale *= 2;
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
                    vfx.UpdateMethod = (_,d) => orb.RotationDegrees += (float)(1000 * d);
                    vfx.OnHit = (() =>
                    {
                        NDebugAudioManager.Instance?.Play("blunt_attack.mp3");
                        VfxCmd.PlayVfx(targetPos.VfxSpawnPosition,"vfx/vfx_attack_blunt");
                    });
                    startPos = targetPos;
                    await vfx.HitTask;
                }
                IEnumerable<Creature> targets = Owner.Creature.HasPower<DiffusiveBoundaryPower>()?CombatState.HittableEnemies:[target];
                var results=await CreatureCmd.Damage(choiceContext,targets,DynamicVars.Damage,Owner.Creature, this);
                dummy.AddResultsInternal(results);
            }
            await Hook.AfterAttack(CombatState,dummy);
            CombatManager.Instance.History.CreatureAttacked(CombatState, Owner.Creature, dummy.Results.ToList());
        }

        public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props,
            Creature target, CardModel cardSource)
        {
            if (cardSource==this)
            {
                await PowerCmd.Apply<VulnerablePower>(target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
            }
        }

        protected override void OnUpgrade() {
            DynamicVars.Repeat.UpgradeValueBy(1);
        }
    }
}
