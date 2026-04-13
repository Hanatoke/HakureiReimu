using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace HakureiReimu.HakureiReimuMod.Relics
{
    public class UnluckyTalisman:AbstractRelic
    {
        public override RelicRarity Rarity => RelicRarity.Common;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SealPower>()];
        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            int amount;
            if (creature is {IsMonster:true, CombatState: not null } && (amount=creature.GetPowerAmount<SealPower>())>0)
            {
                List<Creature> targets = creature.CombatState.HittableEnemies.ToList();
                Flash(targets);
                await PowerCmd.Apply<SealPower>(targets, amount, Owner.Creature,null);
            }
        }
    }
}