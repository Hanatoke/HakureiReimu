using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class ProtectionFeePower : AbstractPower
    {
        public static readonly string ID = nameof(ProtectionFeePower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        private Vector2? _vfxPosition;
        public override Task BeforeDeath(Creature creature)
        {
            _vfxPosition=NCombatRoom.Instance?.GetCreatureNode(creature)?.VfxSpawnPosition;
            return Task.CompletedTask;
        }

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (Owner.IsPlayer&&!wasRemovalPrevented&&creature.IsMonster&&creature.Side==CombatSide.Enemy&&creature.Powers.All(p=>p.ShouldOwnerDeathTriggerFatal()))
            {
                if (_vfxPosition != null)
                {
                    VfxCmd.PlayVfx(_vfxPosition.Value, "vfx/vfx_coin_explosion_regular");
                }
                await PlayerCmd.GainGold(Amount, Owner.Player);
            }
            _vfxPosition=null;
        }
    }
}