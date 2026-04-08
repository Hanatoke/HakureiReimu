using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HakureiReimu.HakureiReimuMod.Powers
{
    public class SoraTobuFushigiNoMikoPower : TemporaryDexterityPower,ICustomPower
    {
        public static readonly string ID = nameof(SoraTobuFushigiNoMikoPower);
        public override AbstractModel OriginModel => ModelDb.Card<SoraTobuFushigiNoMiko>();
        public string CustomBigIconPath {get {
            var path = $"{StringHelper.Unslugify(Id.Entry.RemovePrefix())}.png".PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }}
        public string CustomPackedIconPath => CustomBigIconPath;
    }
}