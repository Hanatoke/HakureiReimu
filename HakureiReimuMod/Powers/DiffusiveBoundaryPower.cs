using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Patches;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

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
                Traverse traverse = Traverse.Create(command);
                traverse.Field<Creature>("_singleTarget").Value = null;
                traverse.Field<CombatState>("_combatState").Value = CombatState;
                traverse.Property<bool>("IsRandomlyTargeted").Value=false;
                traverse.Property<CombatSide>("TargetSide").Value =
                    Owner.Side == CombatSide.Enemy ? CombatSide.Player : CombatSide.Enemy;
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