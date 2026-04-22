using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using HakureiReimu.HakureiReimuMod.Character;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Helpers;

namespace HakureiReimu.HakureiReimuMod.Potions;

[Pool(typeof(HakureiReimuPotionPool))]
public abstract class AbstractPotion : CustomPotionModel
{
    public override string CustomPackedImagePath =>
        $"{StringHelper.Unslugify(Id.Entry.RemovePrefix())}.png".PotionsImagePath();

    public override string CustomPackedOutlinePath =>"Default.png".PotionsImagePath();
}