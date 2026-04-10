using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using HakureiReimu.HakureiReimuMod.Interface.Counter.Hook;
using HakureiReimu.HakureiReimuMod.PersistCard;
using HakureiReimu.HakureiReimuMod.PersistCard.Commands;
using HakureiReimu.HakureiReimuMod.PersistCard.Interface;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class RetainEnergyPower : AbstractPower
    {
        public static readonly string ID = nameof(RetainEnergyPower);

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool ShouldPlayerResetEnergy(Player player) => player.Creature.CombatState.RoundNumber == 1|| player != Owner.Player;
        public override async Task AfterEnergyReset(Player player)
        {
            if (player==Owner.Player)
            {
                await PowerCmd.TickDownDuration(this);
            }
        }
    }
}