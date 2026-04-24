using System.Threading.Tasks;
using Godot;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.PersistCard;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HakureiReimu.HakureiReimuMod.Cards
{
    public abstract class EffectFollowCard<T>(int cost, CardType type, CardRarity rarity, TargetType target,bool showInCardLibrary = true,
        bool autoAdd = true) :AbstractCounterCard(cost, type, rarity, target,showInCardLibrary, autoAdd)
        where T : Node2D
    {
        protected abstract T CreateInstance { get; }
        
        public void CreateEffect(int amount)
        {
            if (!FollowDanmakuManager.CanAddFollow)return;
            for (var i = 0; i < amount; i++)
            {
                CreateEffect();
            }
        }
        public virtual void CreateEffect(bool ignoreMaxLimit=false)
        {
            this.AddFollow(CreateInstance,ignoreMaxLimit:ignoreMaxLimit);
        }
        public virtual T UseEffect()
        {
            if (this.TryGetFollow(out T danmaku))
            {
                return danmaku;
            }
            CreateEffect(true);
            if (this.TryGetFollow(out T temp))
            {
                return temp;
            }
            return null;
        }

        public override Task OnStartPersistCard(AbstractPersistCardSlot slot)
        {
            if (slot.Card==this)
            {
                CreateEffect(slot.Count);
            }
            return base.OnStartPersistCard(slot);
        }

        public override Task OnStopPersistCard(AbstractPersistCardSlot slot)
        {
            if (slot.Card == this)
            {
                this.ClearFollows();
            }
            return base.OnStopPersistCard(slot);
        }

        public override Task AfterModifyPersistCount(AbstractPersistCardSlot slot, int result)
        {
            if (slot.Card == this&&this.GetFollows().Count<result)
            {
                CreateEffect(result-slot.Count);
            }
            return base.AfterModifyPersistCount(slot, result);
        }
    }
}