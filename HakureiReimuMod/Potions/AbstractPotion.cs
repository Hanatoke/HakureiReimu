using BaseLib.Abstracts;
using BaseLib.Utils;
using HakureiReimu.HakureiReimuMod.Character;

namespace HakureiReimu.HakureiReimuMod.Potions;

[Pool(typeof(HakureiReimuPotionPool))]
public abstract class AbstractPotion : CustomPotionModel;