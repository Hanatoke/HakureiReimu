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
    public class HakureiGohei:AbstractRelic
    {
        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Orbs", 1)];
        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side != Owner.Creature.Side) return;
            Flash();
            await YinYangOrbCmd.Spawn(new BlockingPlayerChoiceContext(), Owner, DynamicVars["Orbs"].IntValue);
        }
        public override RelicModel GetUpgradeReplacement()
        {
            return ModelDb.Relic<MoriyaGohei>();
        }
    }
}