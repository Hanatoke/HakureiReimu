using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Command;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class YinYangKijinGyokuPower : AbstractPower
    {
        public static readonly string ID = nameof(YinYangKijinGyokuPower);

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;
        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner.Creature==Owner)
            {
                await YinYangOrbCmd.Spawn(context, Owner.Player, Amount);
            }
        }
    }
}