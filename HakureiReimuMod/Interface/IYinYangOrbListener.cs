using System.Threading.Tasks;
using HakureiReimu.HakureiReimuMod.Core;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Interface
{
    public interface IYinYangOrbListener
    {
        decimal ModifyEvokeVal(YinYangOrb orb,decimal result);
        Task AfterEvokeOrb(PlayerChoiceContext choiceContext,YinYangOrb orb,Player player,Creature target,CardModel cardSource);
    }
}