using BaseLib.Extensions;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rewards;

namespace HakureiReimu.HakureiReimuMod.CombatReward
{
    public abstract class AbstractReward(Player player) :Reward(player)
    {
        protected virtual string LocId => this.GetType().GetPrefix() + StringHelper.Slugify(this.GetType().Name);
        protected LocString DefaultDescription => new("gameplay_ui", LocId+".description");
        public override LocString Description => DefaultDescription;
        protected override string IconPath =>this.GetType().Name.UiImagePath()+".png";
        
    }
}