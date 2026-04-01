using BaseLib.Abstracts;
using Godot;

namespace HakureiReimu.HakureiReimuMod.Character;

public class HakureiReimuPotionPool : CustomPotionPoolModel {
    public override string EnergyColorName => HakureiReimu.CharacterId;
    public override Color LabOutlineColor => HakureiReimu.Color;
}