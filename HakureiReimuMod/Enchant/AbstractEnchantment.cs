using BaseLib.Abstracts;
using BaseLib.Extensions;
using HakureiReimu.HakureiReimuMod.Extensions;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace HakureiReimu.HakureiReimuMod.Enchant
{
    public abstract class AbstractEnchantment :EnchantmentModel,ICustomModel
    {
        public virtual string CustomIcon => $"{StringHelper.Unslugify(Id.Entry.RemovePrefix())}.png".EnchantmentPath();
        
        
        [HarmonyPatch(typeof(EnchantmentModel),nameof(EnchantmentModel.IntendedIconPath), MethodType.Getter)]
        static class IntendedIconPathPatch
        {
            [HarmonyPrefix]
            static bool Prefix(EnchantmentModel __instance,ref string __result)
            {
                if (__instance is AbstractEnchantment enchantment)
                {
                    __result = enchantment.CustomIcon;
                    return false;
                }
                return true;
            }
        }
    }
}