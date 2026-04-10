using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Relics
{
    public class MoriyaGohei:AbstractRelic
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar( 1)];
        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side != Owner.Creature.Side) return;
            Flash();
            
        }
    }
}