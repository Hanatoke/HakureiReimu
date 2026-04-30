using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards.Attack.Rare {
    public class Combo : AbstractCard
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [
                new DamageVar(6, ValueProp.Move)
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [new HoverTip(this.TitleLocString,TipLoc)];
        public override Character.HakureiReimu.Animation Animation => Character.HakureiReimu.Animation.AttackDashLight;
        public Combo(
            ) : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) {
        }

        private static LocString _tipLoc;
        public LocString TipLoc => _tipLoc ??= new LocString("cards", this.Id.Entry + ".tip");

        public static CardModel LastAttack=>CombatManager.Instance.History.CardPlaysFinished.Reverse()
            .FirstOrDefault(e => e.CardPlay.Card.Type == CardType.Attack && e.CardPlay.Card is not Combo&&!e.CardPlay.Card.IsDupe)
            ?.CardPlay.Card;

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx(VfxCmd.slashPath)
                .Execute(choiceContext);
            CardModel card = LastAttack;
            if (card != null)
            {
                if (card.Owner != this.Owner)
                {
                    Player originalOwner = card.Owner;
                    card = card.CreateDupe();
                    card.Owner = null;
                    card.Owner = this.Owner;
                    if (NCard.FindOnTable(card)==null&&NCombatRoom.Instance?.GetCreatureNode(originalOwner.Creature) is {} creature)
                    {
                        NCard nCard = NCard.Create(card);
                        
                        NCombatRoom.Instance?.Ui.PlayContainer.AddChildSafely(nCard);
                        nCard.GlobalPosition = creature.VfxSpawnPosition;
                        nCard.Scale = 0.5f*Vector2.One;
                    }
                }
                await CardCmd.AutoPlay(choiceContext, card, cardPlay.Target);
            }
        }

        protected override void AddExtraArgsToDescription(LocString description)
        {
            bool show=CombatManager.Instance.IsInProgress&&this.Pile is {IsCombatPile:true}&&LastAttack!=null;
            description.Add("HasLastAttack",show);
            if (!show)return;
            if (LastAttack is {} card)
            {
                bool isSelf = card.Owner == this.Owner;
                description.Add("IsSelf",isSelf);
                string name = "";
                if (!isSelf)
                {
                    try
                    {
                        name = PlatformUtil.GetPlayerName(PlatformUtil.PrimaryPlatform, card.Owner.NetId);
                    }
                    catch (Exception)
                    {
                        name = card.Owner.Character.Title.GetFormattedText();
                    }
                }
                description.Add("Owner",name);
                description.Add("LastAttack",card.Title);
            }
        }

        protected override void OnUpgrade() {
            DynamicVars.Damage.UpgradeValueBy(3);
        }
    }
}
