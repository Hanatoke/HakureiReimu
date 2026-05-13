using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class SealingFormationPower : AbstractPower
    {
        public static readonly string ID = nameof(SealingFormationPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (Owner.IsPlayer&&Owner.Player==player)
            {
                FlyingVFXCmd.AddVFXOnCreature(NNova.Create(3,Colors.BlueViolet),Owner);
                Flash();
                foreach (Creature t in CombatState.HittableEnemies)
                {
                    await PowerCmd.Apply<SealPower>(choiceContext, t, Amount, Owner, null);
                }
            }
        }
    }
}