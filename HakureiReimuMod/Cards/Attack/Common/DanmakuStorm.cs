using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Command;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class DanmakuStorm : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3,ValueProp.Move),new RepeatVar(2)];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.ShotA;
        public DanmakuStorm(
            ) : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) {
            SfxCmd.Play("event:/sfx/characters/silent/silent_dagger_spray");
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars.Repeat.IntValue)
                .FromCard(this).TargetingAllOpponents(CombatState)
                .BeforeDamage(async delegate
                {
                    List<Task> tasks = [];
                    foreach (Creature target in CombatState.HittableEnemies)
                    {
                        tasks.Add(FlyingVFXCmd.DanmakuLineToTarget(Owner.Creature,target,(float)GD.RandRange(0.6,0.8),0.3f));
                    }

                    if (tasks.Count>0)
                    {
                        await Task.WhenAny(tasks);
                    }
                })
                .Execute(choiceContext);
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(1);
        }
    }
}
