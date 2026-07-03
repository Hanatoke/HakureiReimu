using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using HakureiReimu.HakureiReimuMod.Cards.Attack.Rare;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Common {
    public class Seal : AbstractCard,ITranscendenceCard
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<Powers.SealPower>()];
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new DamageVar(3, ValueProp.Move), new PowerVar<Powers.SealPower>(3)];

        public CardModel GetTranscendenceTransformedCard() => ModelDb.Card<DreamSealingDivine>();

        public Seal(
            ) : base(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .Execute(choiceContext);
            await PowerCmd.Apply<Powers.SealPower>(choiceContext,cardPlay.Target, DynamicVars[Powers.SealPower.ID].BaseValue,
                Owner.Creature, this);
        }
        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(1);
            DynamicVars[Powers.SealPower.ID].UpgradeValueBy(1);
        }
    }
}
