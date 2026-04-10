using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Rare {
    public class DivineMight : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<ArtifactPower>(1)
        ];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromPower<ArtifactPower>()
        ];
        public DivineMight(
            ) : base(1, CardType.Power, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            foreach (PowerModel p in Owner.Creature.Powers.ToList())
            {
                if (p.TypeForCurrentAmount==PowerType.Debuff&&p.IsVisible)
                {
                    await PowerCmd.Remove(p);
                }
            }
            await PowerCmd.Apply<ArtifactPower>(Owner.Creature,
                DynamicVars[nameof(ArtifactPower)].BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade() 
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
