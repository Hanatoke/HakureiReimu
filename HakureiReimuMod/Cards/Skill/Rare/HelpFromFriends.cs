using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace HakureiReimu.HakureiReimuMod.Cards.Skill.Rare {
    public class HelpFromFriends : AbstractCard
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => 
            [
                CardKeyword.Exhaust,
            ];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromCard<Procrastinate>(IsUpgraded)
        ];

        public HelpFromFriends(
            ) : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self) {
        }
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            List<CardModel> choose = GenerateChoose(RunState.Rng.CombatCardGeneration)
                .Select(c => CombatState.CreateCard(c, Owner)).ToList();
            if (IsUpgraded)choose.ForEach(c=>CardCmd.Upgrade(c));
            CardModel c = (choose.Count > 0
                ? (await CardSelectCmd.FromChooseACardScreen(choiceContext, choose, Owner, true))
                : null);
            if (c == null)
            {
                c = CombatState.CreateCard(ModelDb.Card<Procrastinate>(), Owner);
                if (IsUpgraded)CardCmd.Upgrade(c);
            }

            await CardPileCmd.AddGeneratedCardToCombat(c, PileType.Hand, Owner);
        }

        public List<CardModel> GenerateChoose(Rng rng)
        {
            List<CardModel> all = AllChoose;
            List<CardModel> result = [];
            if (all.Count <= 0) return result;
            AddType(CardType.Attack);
            AddType(CardType.Skill);
            AddType(CardType.Power);
            return result;

            void AddType(CardType type)
            {
                List<CardModel> pool = all.Where(c => c.Type == type && !result.Contains(c)).ToList();

                if (pool.Count <= 0)
                {
                    pool = all.Where(c => !result.Contains(c)).ToList();
                }
                if (pool.Count > 0)
                {
                    result.Add(rng.NextItem(pool));
                }
            }
        }

        protected override void AddExtraArgsToDescription(LocString description)
        {
            base.AddExtraArgsToDescription(description);
            description.Add("HasFriend",AllChoose.Count>0);
        }

        private static List<CardModel> _allChoose;
        public static List<CardModel> AllChoose
        {
            get
            {
                if (_allChoose == null)
                {
                    _allChoose = [];
                    foreach (var s in CardId)
                    {
                        CardModel c = ModelDb.GetByIdOrNull<CardModel>(s);
                        if (c != null)
                        {
                            _allChoose.Add(c);
                        }
                    }
                }
                return  _allChoose;
            }
        }
        
        public static readonly List<ModelId> CardId = [
            //魔理沙
            new ("CARD","MARISAMOD-BUTT_SMASH"),//尻击
            new ("CARD","MARISAMOD-FAIRY_DESTRUCTION_RAY"),//妖精手电筒
            new ("CARD","MARISAMOD-DARK_SPARK"),//暗色火花
            
            new ("CARD","MARISAMOD-POWER_UP"),//强化
            new ("CARD","MARISAMOD-CHARGING_UP"),//魔炮准备
            new ("CARD","MARISAMOD-STAR_DUST_REVERIE"),//星屑幻想
            
            new ("CARD","MARISAMOD-PROP_BAG"),//便携道具包
            new ("CARD","MARISAMOD-SINGULARITY"),//奇点
            new ("CARD","MARISAMOD-CASKET_OF_STAR"),//星之器
            
            //八云紫 暂未发现
            
            //爱丽丝 暂未写完
            
        ];
    }
}
