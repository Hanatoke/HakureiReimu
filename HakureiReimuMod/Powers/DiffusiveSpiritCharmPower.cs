using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Node.VFX;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class DiffusiveSpiritCharmPower : AbstractPower
    {
        public static readonly string ID = nameof(DiffusiveSpiritCharmPower);
        public static string Path = "diffusive.tscn".ScenePath();

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public bool IsInApply;

        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side==Owner.Side)
            {
                await PowerCmd.TickDownDuration(this);
            }
        }

        public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature applier, CardModel cardSource)
        {
            if (!IsInApply && applier == Owner && power.GetTypeForAmount(amount) == PowerType.Debuff)
            {
                IsInApply = true;
                FlyingVFXCmd.AddVFXOnCreature(PreloadManager.Cache.GetScene(Path).Instantiate<NVfxParticleOneShot>(),power.Owner);
                Flash();
                List<Creature> targets = Owner.Side != CombatSide.Enemy
                    ? CombatState.HittableEnemies.ToList()
                    : CombatState.PlayerCreatures.Where(p=>p.IsHittable).ToList();
                foreach (Creature t in targets)
                {
                    if (t!=power.Owner)
                    {
                        await PowerCmd.Apply((PowerModel)power.ClonePreservingMutability(), t, amount, Owner, null);
                    }
                }
                IsInApply = false;
            }
        }
    }
}