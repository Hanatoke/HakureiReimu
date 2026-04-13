using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface.Counter;
using HakureiReimu.HakureiReimuMod.Interface.Counter.Hook;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Relics
{
    public class AutonomousYinYangJade:AbstractRelic,ICounterListener
    {
        public override RelicRarity Rarity => RelicRarity.Rare;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromKeyword(AbstractCard.Counter),
            HoverTipFactory.FromOrb<YinYangOrb>()
        ];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Orbs", 1)];
        public async Task AfterCounter(CombatState state, ICounter counter, Creature target)
        {
            if (counter.CounterOwner==Owner.Creature)
            {
                await YinYangOrbCmd.Spawn(new BlockingPlayerChoiceContext(), Owner, DynamicVars["Orbs"].IntValue);
            }
        }
    }
}