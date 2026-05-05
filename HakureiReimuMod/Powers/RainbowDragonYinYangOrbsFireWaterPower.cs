using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface;
using HakureiReimu.HakureiReimuMod.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class RainbowDragonYinYangOrbsFireWaterPower : AbstractPower,IYinYangOrbListener
    {
        public static readonly string ID = nameof(RainbowDragonYinYangOrbsFireWaterPower);
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>()];

        public void ModifyOrbDamage(PlayerChoiceContext choiceContext, YinYangOrb orb, List<Creature> targets, ref decimal damage,
            ref ValueProp props)
        {
            if (orb.Owner.Creature==Owner)
            {
                props |= ValueProp.Unblockable;
                props |= DamagePropsPatch.IgnoreDamageImmunity;
                props |= DamagePropsPatch.IgnoreDamageResponse;
            }
        }
    }
    public class RainbowDragonYinYangOrbsStormMountainPower : AbstractPower,IYinYangOrbListener
    {
        public static readonly string ID = nameof(RainbowDragonYinYangOrbsStormMountainPower);
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>()];
        public async Task AfterOrbHit(PlayerChoiceContext choiceContext, YinYangOrb orb, IEnumerable<DamageResult> damageResult)
        {
            if (orb.Owner.Creature==Owner)
            {
                foreach (DamageResult result in damageResult)
                {
                    if (result.UnblockedDamage>0)
                    {
                        await CreatureCmd.GainBlock(Owner, result.UnblockedDamage, ValueProp.Unpowered, null, true);
                    }
                }
            }
        }
    }
    public class RainbowDragonYinYangOrbsWindThunderPower : AbstractPower,IYinYangOrbListener
    {
        public static readonly string ID = nameof(RainbowDragonYinYangOrbsWindThunderPower);
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>()];

        public void ModifyOrbDamage(PlayerChoiceContext choiceContext, YinYangOrb orb, List<Creature> targets, ref decimal damage,
            ref ValueProp props)
        {
            if (orb.Owner.Creature==Owner&&targets.Count==1)
            {
                Creature t = targets[0];
                targets.Clear();
                targets.AddRange(CombatState.GetCreaturesOnSide(t.Side));
            }
        }
    }
}