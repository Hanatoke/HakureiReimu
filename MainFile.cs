using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using Godot;
using HakureiReimu.HakureiReimuMod.Cards;
using HakureiReimu.HakureiReimuMod.Command;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Patches;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;

namespace HakureiReimu;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "HakureiReimu"; //Used for resource filepath

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);
        harmony.PatchAll();
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(typeof(MainFile).Assembly);
        // foreach (CardModel c in ModelDb.AllCards.Where(c=>c is AbstractCard))
        // {
        //     SaveManager.Instance.Progress.MarkCardAsSeen(c.Id);
        // }
        ModHelper.SubscribeForCombatStateHooks(ModId,CombatHookSubscription );
    }

    public static IEnumerable<AbstractModel> CombatHookSubscription(CombatState state)
    {
        bool alreadyFind = false;
        foreach (Player p in state.Players)
        {
            if (!alreadyFind && p.Creature.CombatState != null && p.Character is HakureiReimuMod.Character.HakureiReimu c)
            {
                alreadyFind = true;
                yield return c;
            }
            if (p.PlayerCombatState != null)
            {
                YinYangOrbManager m=YinYangOrbPatch.Managers[p.PlayerCombatState];
                if (m != null)
                {
                    yield return m;
                    foreach (YinYangOrb orb in m.Orbs)
                    {
                        if (!orb.HasBeenRemovedFromState && orb.Owner.IsActiveForHooks)
                        {
                            yield return orb;
                        }
                    }
                }
            }
        }
    }
}
