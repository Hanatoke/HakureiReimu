using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HakureiReimu.HakureiReimuMod.Cards.Power.Uncommon {
    public class DivinePossession : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars => 
            [
                new PowerVar<DivinePossessionPower>(1)
            ];

        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

        public DivinePossession(
            ) : base(1, CardType.Power, CardRarity.Uncommon, TargetType.AnyAlly) {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            Player player = cardPlay.Target?.Player;
            if (player == null)return;
            await PowerCmd.Apply<DivinePossessionPower>(choiceContext, player.Creature,
                DynamicVars[DivinePossessionPower.ID].BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Innate);
        }
    }
}
