using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Cards.Skill.Rare;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class BulletMoneyBonusPacketsPower : AbstractPower
    {
        public static readonly string ID = nameof(BulletMoneyBonusPacketsPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public override bool IsInstanced => true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [HoverTipFactory.FromCard<BulletMoneyBonusPackets>()];

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new StringVar("Applier")
        ];

        public override Task AfterApplied(Creature applier, CardModel cardSource)
        {
            ((StringVar) this.DynamicVars["Applier"]).StringValue = PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, this.Applier.Player.NetId);
            return Task.CompletedTask;
        }

        public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side==Owner.Side)
            {
                await PowerCmd.TickDownDuration(this);
            }
        }

        public override int ModifyAttackHitCount(AttackCommand attack, int hitCount)
        {
            if (attack.Attacker == Owner && attack.DamageProps.HasFlag(ValueProp.Move)) 
            {
                Flash();
                return hitCount * 2;
            }
            return hitCount;
        }
    }
}