using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HakureiReimu.HakureiReimuMod.Node.VFX.Mover;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public class YinYangOrb :CustomOrbModel
    {
        public static readonly string ImgPath = "effects/YinYangOrb.png".ImagePath();
        public static readonly string ScenePath = "yin_yang_orb.tscn".ScenePath();
        public override decimal PassiveVal => EvokeVal;
        public override decimal EvokeVal => ModifyEvokeVal(3);
        public override Color DarkenedColor => new(1, 1, 1, 0.5f);

        public virtual decimal ModifyEvokeVal(decimal result)=>YinYangOrbHook.ModifyOrbValue(this, result);
        protected override string ChannelSfx => "event:/sfx/characters/defect/defect_dark_channel";
        public override string CustomIconPath => ImgPath;

        public override Node2D CreateCustomSprite()
        {
            return PreloadManager.Cache.GetScene(ScenePath).Instantiate<Node2D>();
        }

        public override Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext playerChoiceContext)
        {
            return Task.FromResult<IEnumerable<Creature>>(new List<Creature>());
        }
        public virtual async Task<IEnumerable<DamageResult>> Attack(PlayerChoiceContext playerChoiceContext,Creature target,NOrb nOrb,Vector2 startPos)
        {
            if (target is { IsHittable: false })
            {
                target = null;
            }
            if (target == null)
            {
                var enemies = CombatState.GetOpponentsOf(Owner.Creature)
                    .Where(e => e.IsHittable).ToList();
                 target = Owner.RunState.Rng.CombatTargets.NextItem(enemies)!;
            }
            if (target == null) return [];
            List<Creature> targets = [target];
            decimal damage = EvokeVal;
            ValueProp props = ValueProp.Unpowered;
            YinYangOrbHook.ModifyOrbDamage(playerChoiceContext,this,targets,ref damage,ref props);
            
            NCreature targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
            if (targetNode!=null)
            {
                Vector2 targetPos = targetNode.VfxSpawnPosition;
                Vector2 v = Vector2.One.Rotated(GD.Randf() * Mathf.Tau) * 100;
                FlyingVFX vfx=FlyingVFX.Create(new SteeringMover(startPos,targetPos,v));
                vfx.Duration = 2;
                vfx.OnHit = () =>
                {
                    NDebugAudioManager.Instance?.Play("blunt_attack.mp3");
                    VfxCmd.PlayOnCreatureCenter(target,"vfx/vfx_attack_blunt");
                };
                vfx.AddChildSafely(NYinYangOrbFlying.Create());
                NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
            }
            
            List<DamageResult> results=(await CreatureCmd.Damage(playerChoiceContext, targets, damage, props, Owner.Creature)).ToList();
            await YinYangOrbHook.AfterOrbHit(playerChoiceContext,this,results);
            return results;
        }
    }
}