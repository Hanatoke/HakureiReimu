using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using HakureiReimu.HakureiReimuMod.Character;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Helpers;

namespace HakureiReimu.HakureiReimuMod.Relics;

[Pool(typeof(HakureiReimuRelicPool))]
public abstract class AbstractRelic : CustomRelicModel {
    public override string PackedIconPath {
        get {
            var path = $"{StringHelper.Unslugify(Id.Entry.RemovePrefix())}.png".RelicImagePath();
            return ResourceLoader.Exists(path) ? path : "relic.png".RelicImagePath();
        }
    }

    protected override string PackedIconOutlinePath {
        get {
            var path = "Default.png".RelicImagePath();
            return ResourceLoader.Exists(path) ? path : "relic_outline.png".RelicImagePath();
        }
    }

    protected override string BigIconPath {
        get {
            var path = $"{StringHelper.Unslugify(Id.Entry.RemovePrefix())}.png".BigRelicImagePath();
            return ResourceLoader.Exists(path) ? path : "relic.png".BigRelicImagePath();
        }
    }
}