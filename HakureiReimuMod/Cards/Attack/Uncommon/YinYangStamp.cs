using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Uncommon {
    public class YinYangStamp : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DamageVar(6,ValueProp.Move),
                new EnergyVar(2)
            ];

        protected override bool ShouldGlowGoldInternal =>CombatManager.Instance.History.Entries.OfType<PowerReceivedEntry>().Any(e =>
            e.HappenedThisTurn(CombatState) && e.Applier == Owner.Creature &&
            e.Power.GetTypeForAmount(e.Amount) == PowerType.Debuff);

        public YinYangStamp(
            ) : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .BeforeDamage(delegate
                {
                    SfxCmd.Play("event:/sfx/characters/attack_fire");
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(cardPlay.Target));
                    return Task.CompletedTask;
                })
                .Execute(choiceContext);
            if (ShouldGlowGoldInternal)
            {
                await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
            }
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(3);
        }
    }
}
