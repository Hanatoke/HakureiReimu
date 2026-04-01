using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

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
        public virtual Task Attack(Creature taget)
        {
            // var enemies = CombatState.GetOpponentsOf(Owner.Creature)
            //     .Where(e => e.IsHittable).ToList();
            // if (enemies.Count == 0) return enemies;
            // Creature target = Owner.RunState.Rng.CombatTargets.NextItem(enemies)!;
            return Task.CompletedTask;
        }
    }
}