using BaseLib.Abstracts;
using Godot;

namespace HakureiReimu.HakureiReimuMod.Character;

public class HakureiReimuRelicPool : CustomRelicPoolModel {
    public override string EnergyColorName => HakureiReimu.CharacterId;
    public override Color LabOutlineColor => HakureiReimu.Color;
}