using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class DreamSubspaceHole : AbstractCard {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new BlockVar(6,ValueProp.Move),
                new CardsVar(3)
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Retain)];
        public override bool GainsBlock => true;
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.SpellFastA;
        public DreamSubspaceHole(
            ) : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature,DynamicVars.Block,cardPlay);
            CardSelectorPrefs prefs = new (SelectionScreenPrompt,0,DynamicVars.Cards.IntValue)
            {
                Cancelable = true,
            };
            List<CardModel> cards =
                (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, c => !c.ShouldRetainThisTurn, this))
                .ToList();
            foreach (CardModel card in cards)
            {
                card.GiveSingleTurnRetain();
            }
            
            // await PowerCmd.Apply<ExtraDrawPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
        }
        protected override void OnUpgrade() {
            DynamicVars.Block.UpgradeValueBy(2);
            DynamicVars.Cards.UpgradeValueBy(1);
        }
    }
}
