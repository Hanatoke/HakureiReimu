using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon {
    public class Duel : AbstractCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => 
            [EnergyHoverTip];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new EnergyVar(3)];
        
        public Duel(
            ) : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target is {IsMonster:true,Monster: { } t})
            {
                await t.PerformMove();
                t.RollMove(CombatState.PlayerCreatures);
                NCombatRoom.Instance?.GetCreatureNode(t.Creature)?.RefreshIntents();
            }
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
        protected override void OnUpgrade() {
            AddKeyword(CardKeyword.Retain);
        }
    }
}
