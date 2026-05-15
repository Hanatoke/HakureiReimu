using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using HakureiReimu.HakureiReimuMod.Cards.Skill.Uncommon;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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

        public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
        {
            return Task.CompletedTask;
        }

        public override async Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
        {
            if (side != Owner.Side)
                return;
            Flash();
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<DexterityPower>(new BlockingPlayerChoiceContext(), Owner, -Amount, Owner, null);
        }
    }
}