using HakureiReimu.HakureiReimuMod.Node;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HakureiReimu.HakureiReimuMod.Extensions
{
    public static class PlayerExtensions
    {
        public static void RunAnimation(this Player p, Character.HakureiReimu.Animation animation)
        {
            if (p.Character is Character.HakureiReimu c)
            {
                NCreature nC = NCombatRoom.Instance?.GetCreatureNode(p.Creature);
                if (nC is { Visuals: HakureiReimuVisuals vn })
                {
                    vn.Playback?.Travel(animation.Name());
                }
            }
        }
    }
}