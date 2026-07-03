using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface;
using HakureiReimu.HakureiReimuMod.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class RainbowDragonYinYangOrbsFireWaterPower : AbstractPower,IYinYangOrbListener
    {
        public static readonly string ID = nameof(RainbowDragonYinYangOrbsFireWaterPower);
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<YinYangOrb>()];

        public static readonly ValueProp ModifyProp = ValueProp.Unblockable | DamagePropsPatch.IgnoreDamageImmunity |
                                                      DamagePropsPatch.IgnoreDamageResponse;

        public void ModifyOrbDamage(PlayerChoiceContext choiceContext, YinYangOrb orb, List<Creature> targets, ref decimal damage,
            ref ValueProp props)
        {
            if (orb.Owner.Creature==Owner)
            {
                props |= ModifyProp;
            }
        }

        public static readonly ValueProp CalculateProp = ValueProp.Move;

        public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature dealer,
            CardModel cardSource, CardPlay cardPlay)
        {
            if (dealer == Owner && (props & DamagePropsPatch.YinYangOrbDamage) != 0)
            {
                decimal total = 0;
                foreach (AbstractModel listener in CombatState.IterateHookListeners())
                {
                    if (listener is not (RainbowDragonYinYangOrbsFireWaterPower or StrengthPower))
                    {
                        decimal add = listener.ModifyDamageAdditive(target, amount, CalculateProp, dealer, cardSource,cardPlay);
                        if (add > 0)
                        {
                            total += add;
                        }
                    }
                }
                return total;
            }
            return 0;
        }

        public override decimal ModifyDamageMultiplicative(Creature target, decimal amount, ValueProp props, Creature dealer,
            CardModel cardSource, CardPlay cardPlay)
        {
            if (dealer == Owner && (props & DamagePropsPatch.YinYangOrbDamage) != 0)
            {
                decimal total = 1;
                foreach (AbstractModel listener in CombatState.IterateHookListeners())
                {
                    if (listener is not (RainbowDragonYinYangOrbsFireWaterPower or StrengthPower))
                    {
                        decimal mult = listener.ModifyDamageMultiplicative(target, amount, CalculateProp, dealer, cardSource,cardPlay);
                        if (mult > 1)
                        {
                            total *= mult;
                        }
                    }
                }
                return total;
            }
            return 1;
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
                        decimal value = Math.Floor((decimal)result.UnblockedDamage/2);
                        await CreatureCmd.GainBlock(Owner, value, ValueProp.Unpowered, null, true);
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