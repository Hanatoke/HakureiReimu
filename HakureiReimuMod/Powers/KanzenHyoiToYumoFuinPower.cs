using System.Collections.Generic;
using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Interface;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class KanzenHyoiToYumoFuinPower : AbstractPower,IYinYangOrbListener
    {
        public static readonly string ID = nameof(KanzenHyoiToYumoFuinPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromPower<SealPower>()
        ];

        public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props,
            Creature target, CardModel cardSource)
        {
            if (dealer != Owner || !props.IsPoweredAttack() || target.Side == Owner.Side) return;
            await PowerCmd.Apply<SealPower>(choiceContext, target, Amount, Owner, null);
        }
    }
}