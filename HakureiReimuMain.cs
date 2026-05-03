using System.Collections.Generic;
using BaseLib.Config;
using Godot;
using HakureiReimu.HakureiReimuMod.CombatReward;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Patches;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using ModConfig = HakureiReimu.HakureiReimuMod.Core.ModConfig;

namespace HakureiReimu;

[ModInitializer(nameof(Initialize))]
public partial class HakureiReimuMain : Node
{
    public const string ModId = "HakureiReimu"; //Used for resource filepath

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);
        harmony.PatchAll();
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(typeof(HakureiReimuMain).Assembly);
        ModConfigRegistry.Register(ModId,new ModConfig());
        // foreach (CardModel c in ModelDb.AllCards.Where(c=>c is AbstractCard))
        // {
        //     SaveManager.Instance.Progress.MarkCardAsSeen(c.Id);
        // }
        ModHelper.SubscribeForCombatStateHooks(ModId,CombatHookSubscription );
        CombatManager.Instance.CombatSetUp += _ => FollowDanmakuManager.Clear();
        CombatManager.Instance.CombatEnded += _ => FollowDanmakuManager.Clear();
        
        CustomRewardPatch.CustomRewards.Add(BlindBoxReward.Type,(s,p)=>new BlindBoxReward(p));
        CustomRewardPatch.CustomRewards.Add(UpgradeReward.Type,(s,p)=>new UpgradeReward(p));
        CustomRewardPatch.CustomRewards.Add(TransformReward.Type,(s,p)=>new TransformReward(p));
        CustomRewardPatch.CustomRewards.Add(CloneReward.Type,(s,p)=>new CloneReward(p));
        CustomRewardPatch.CustomRewards.Add(EnchantReward.Type,(s,p)=>new EnchantReward(p,s.PredeterminedModelId,s.OptionCount));
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
        if (ScalingModel!=null)
        {
            yield return ScalingModel;
        }
    }

    private static ReimuMultiplayerScalingModel _scalingModel;
    public static ReimuMultiplayerScalingModel ScalingModel=>_scalingModel??=ModelDb.GetById<ReimuMultiplayerScalingModel>(ModelDb.GetId<ReimuMultiplayerScalingModel>());
}
