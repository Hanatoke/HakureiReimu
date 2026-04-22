using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.Potions
{
    public class SealingTalisman :AbstractPotion
    {
        public override PotionRarity Rarity => PotionRarity.Common;
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override TargetType TargetType => TargetType.AnyEnemy;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SealPower>(10)];

        public override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<ArtifactPower>(), HoverTipFactory.FromPower<SealPower>()
        ];

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature target)
        {
            NCombatRoom.Instance?.PlaySplashVfx(target, Colors.MediumPurple);
            if (target.HasPower<ArtifactPower>())
            {
                await PowerCmd.Remove<ArtifactPower>(target);
            }
            await PowerCmd.Apply<SealPower>(target, DynamicVars[SealPower.ID].BaseValue, Owner.Creature, null);
        }
    }
}