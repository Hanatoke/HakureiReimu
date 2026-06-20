using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace HakureiReimu.HakureiReimuMod.PersistCard.Patch
{
    public class NCardPileScreenPatch
    {
        [HarmonyPatch(typeof(NCardPileScreen), nameof(NCardPileScreen._Ready))]
        public static class ReadyPatch
        {
            [HarmonyPostfix]
            public static void Postfix(NCardPileScreen __instance,MegaRichTextLabel ____bottomLabel)
            {
                if (__instance.Pile is AbstractPersistCardTable table&& table.PileViewScreenText(__instance) is {} locString
                    )
                {
                    ____bottomLabel.Text = locString.GetFormattedText();
                    ____bottomLabel.Visible = true;
                }
            }
        }

        [HarmonyPatch(typeof(NCardPileScreen), "OnPileContentsChanged")]
        public static class OnPileContentsChangedPatch
        {
            [HarmonyPostfix]
            public static void Postfix(NCardPileScreen __instance, MegaRichTextLabel ____bottomLabel)
            {
                if (__instance.Pile is AbstractPersistCardTable table&& table.PileViewScreenText(__instance) is {} locString
                   )
                {
                    ____bottomLabel.Text = locString.GetFormattedText();
                    ____bottomLabel.Visible = true;
                }
            }
        }
    }
}