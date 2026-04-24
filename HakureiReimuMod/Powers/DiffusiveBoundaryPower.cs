using System;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class DiffusiveBoundaryPower : AbstractPower
    {
        public static readonly string ID = nameof(DiffusiveBoundaryPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        public override Task BeforeAttack(AttackCommand command)
        {
            if (command.Attacker==Owner)
            {
                Flash();
                bool isAoe = command.IsMultiTargeted;
                Traverse traverse = Traverse.Create(command);
                // FlyingVFXCmd.AddVFXOnCreature(NBarrierImpactReverse.Create(),traverse.Field<Creature>("_singleTarget").Value);
                traverse.Field<Creature>("_singleTarget").Value = null;
                traverse.Field<CombatState>("_combatState").Value = CombatState;
                traverse.Property<bool>("IsRandomlyTargeted").Value=false;
                CombatSide targetSide=traverse.Property<CombatSide>("TargetSide").Value =
                    Owner.Side == CombatSide.Enemy ? CombatSide.Player : CombatSide.Enemy;
                if (isAoe) return Task.CompletedTask;
                Func<Task> old = traverse.Field<Func<Task>>("_beforeDamage").Value;
                traverse.Field<Func<Task>>("_beforeDamage").Value = async () =>
                {
                    if (old != null)
                    {
                        await old();
                    }
                    VfxCmd.PlayOnSide(targetSide,"vfx/vfx_giant_horizontal_slash",CombatState);
                };
            }
            return Task.CompletedTask;
        }

        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side==Owner.Side)
            {
                await PowerCmd.TickDownDuration(this);
            }
        }
    }
}