using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class MenreikiDaiChofuku : AbstractCounterCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [new CounterVar(3)];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [Attack];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellFastA;
        public MenreikiDaiChofuku(
            ) : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) {
        }
        public override int Duration => DynamicVars[CounterVar.DefaultName].IntValue;

        protected override void OnUpgrade() {
            AddKeyword(Immediate);
        }
        public override bool IsImmediate => IsUpgraded;
        public override CounterType ActivateType => CounterType.Attack;

        public override async Task Invoke(Creature target, bool cost = true, bool instant = false)
        {
            if (target is not { IsMonster: true, Monster.IntendsToAttack: true }) return;
            int d = 0;
            foreach (AbstractIntent nextMoveIntent in target.Monster.NextMove.Intents)
            {
                if (nextMoveIntent is AttackIntent attackIntent)
                {
                    d +=(int)CalculateIntentDamage(attackIntent, target, Owner);
                }
            }
            RunAnimation(Character.HakureiReimu.Animation.AttackCloseRound);
            await Flash(instant);
            await DamageCmd.Attack(d).FromCard(this).TargetingAllOpponents(CombatState)
                .WithHitFx("vfx/vfx_giant_horizontal_slash", tmpSfx: "slash_attack.mp3").BeforeDamage(async () =>
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NSweepingBeamVfx.Create(Owner.Creature,CombatState.HittableEnemies.ToList()));
                    await Cmd.Wait(0.25f);
                })
                .Execute(null);
            if (cost)
            {
                await Decrement();
            }
        }

        public static decimal CalculateIntentDamage(AttackIntent intent,Creature owner,Player target)
        {
            decimal d = intent.DamageCalc?.Invoke() ?? 0;
            d = Hook.ModifyDamage(target.RunState, target.Creature.CombatState, target.Creature, owner, d, ValueProp.Move, null,
                ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);
            return d * intent.Repeats;
        }
    }
}
