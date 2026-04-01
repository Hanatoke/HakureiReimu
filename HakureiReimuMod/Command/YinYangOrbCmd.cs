using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HakureiReimu.HakureiReimuMod.Command
{
    public static class YinYangOrbCmd
    {
        public static async Task Spawn(PlayerChoiceContext choiceContext,Player player,int amount = 1)
        {
            await OrbCmd.Channel<YinYangOrb>(choiceContext, player);
        }
        public static Task Evoke(YinYangOrb orb, Creature target)
        {
            return Task.CompletedTask;
        }
    }
}