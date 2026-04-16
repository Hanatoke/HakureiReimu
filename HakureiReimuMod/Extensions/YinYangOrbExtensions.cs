using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Node;
using HakureiReimu.HakureiReimuMod.Patches;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HakureiReimu.HakureiReimuMod.Extensions
{
    public static class YinYangOrbExtensions
    {
        public static YinYangOrbManager YinYangOrbManager(this PlayerCombatState state)
        {
            return YinYangOrbPatch.Managers[state];
        }

        public static NYinYangOrbManager NYinYangOrbManager(this NCreature creature,YinYangOrbManager manager)
        {
            if (manager == null)return null;
            NYinYangOrbManager m=creature.GetNodeOrNull<NYinYangOrbManager>("%"+manager.GetType().Name);
            if (m == null)
            {
                m = Node.NYinYangOrbManager.Create(creature, LocalContext.IsMe(creature.Entity));
                creature.AddChildSafely(m);
                m.Name=manager.GetType().Name;
                m.UniqueNameInOwner = true;
                m.Owner=creature;
            }
            return m;
        }
    }
}