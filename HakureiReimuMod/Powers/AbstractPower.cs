using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Helpers;

namespace HakureiReimu.HakureiReimuMod.Powers;

public abstract class AbstractPower : CustomPowerModel {
    //Loads from CharMod/images/powers/your_power.png
    public override string CustomPackedIconPath {
        get {
            var path = $"{StringHelper.Unslugify(Id.Entry.RemovePrefix())}.png".PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }
    }

    public override string CustomBigIconPath {
        get {
            var path = $"{StringHelper.Unslugify(Id.Entry.RemovePrefix())}.png".PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
        }
    }
}