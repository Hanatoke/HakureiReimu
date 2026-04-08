using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Interface;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class YinYangAsukaIPower : AbstractPower,IYinYangOrbListener
    {
        public static readonly string ID = nameof(YinYangAsukaIPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public async Task AfterOrbHit(PlayerChoiceContext choiceContext, YinYangOrb orb, IEnumerable<DamageResult> damageResult)
        {
            if (orb?.Owner.Creature==Owner)
            {
                foreach (DamageResult result in damageResult)
                {
                    if (!result.WasTargetKilled&&result.UnblockedDamage>0)
                    {
                        Flash();
                        for (var i = 0; i < Amount; i++)
                        {
                            PowerModel p=RollPower?.MutableClone() as PowerModel;
                            if (p != null)
                            {
                                await PowerCmd.Apply(p, result.Receiver, 1, Owner, null);
                            }
                        }
                    }
                }
            }
        }
        public PowerModel RollPower=>CombatState.RunState.Rng.CombatOrbGeneration.NextItem(RandomPower);
        public static readonly PowerModel[] RandomPower = [
            ModelDb.Power<WeakPower>(),
            ModelDb.Power<VulnerablePower>(),
            ModelDb.Power<SealPower>(),
            ModelDb.Power<PoisonPower>(),
            ModelDb.Power<StranglePower>(),
            ModelDb.Power<DemisePower>(),
        ];
    }
}