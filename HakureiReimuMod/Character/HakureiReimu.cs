using BaseLib.Abstracts;
using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using HakureiReimu.HakureiReimuMod.Cards;
using HakureiReimu.HakureiReimuMod.Cards.Attack.Common;
using HakureiReimu.HakureiReimuMod.Cards.Skill.Common;
using HakureiReimu.HakureiReimuMod.Node;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Character;

public class HakureiReimu : PlaceholderCharacterModel {
    public const string CharacterId = "HakureiReimu";

    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 76;

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<BasicStrike>(),
        ModelDb.Card<BasicStrike>(),
        ModelDb.Card<BasicStrike>(),
        ModelDb.Card<BasicStrike>(),
        ModelDb.Card<BasicDefend>(),
        ModelDb.Card<BasicDefend>(),
        ModelDb.Card<BasicDefend>(),
        ModelDb.Card<BasicDefend>(),
        ModelDb.Card<Seal>(),
        ModelDb.Card<BoundaryCreation>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<BurningBlood>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<HakureiReimuCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<HakureiReimuRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<HakureiReimuPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets. 
        These are just some of the simplest assets, given some placeholders to differentiate your character with. 
        You don't have to, but you're suggested to rename these images. */
    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
    //角色模型实例路径
    public override string CustomVisualPath => Path.Join("HakureiReimu","HakureiReimu.tscn").CharacterPath();

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Character is HakureiReimu c)
        {
            NCreature nC = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Card.Owner.Creature);
            if (nC is { Visuals: HakureiReimuVisuals vn })
            {
                AnimationNodeStateMachinePlayback playback = vn.Playback;
                if (cardPlay.Card is AbstractCard card)
                {
                    if (card.Animation!=Animation.None)
                    {
                        playback?.Travel(card.Animation.Name());
                    }
                }
                DefaultCardAnimation(cardPlay,playback);
            }
        }
        return Task.CompletedTask;
    }

    public virtual void DefaultCardAnimation(CardPlay cardPlay,AnimationNodeStateMachinePlayback playback)
    {
        CardModel card = cardPlay.Card;
        Animation animation = Animation.None;
        switch (card.Type)
        {
            case CardType.Attack:
                if (card.DynamicVars.ContainsKey(nameof(CardModel.DynamicVars.Damage))&&card.DynamicVars.Damage.IntValue>=20)
                {
                    animation = Animation.AttackCloseHeavy;
                }
                else
                {
                    animation = Animation.AttackCloseRound;
                }
                break;
            case CardType.Skill:
                if (card.DynamicVars.ContainsKey(nameof(CardModel.DynamicVars.Block))&&card.DynamicVars.Block.BaseValue>1)
                {
                    animation = Animation.Block;
                }
                else
                {
                    animation = Animation.SpellFastA;
                }
                break;
            case CardType.Power:
                if (card.Rarity is CardRarity.Rare or CardRarity.Ancient)
                {
                    animation = Animation.SpellLongB;
                }
                else
                {
                    animation = Animation.SpellLongA;
                }
                break;
        }
        playback?.Travel(animation.Name());
    }

    public override Task AfterDamageReceivedLate(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
        Creature dealer, CardModel cardSource)
    {
        if (target!=dealer&& target.IsPlayer&&target.Player?.Character is HakureiReimu c)
        {
            NCreature nC = NCombatRoom.Instance?.GetCreatureNode(target);
            if (nC is { Visuals: HakureiReimuVisuals vn })
            {
                AnimationNodeStateMachinePlayback playback = vn.Playback;
                if (result.WasFullyBlocked)
                {
                    playback?.Travel(Animation.Guard.Name());
                }else if (result.WasBlockBroken)
                {
                    playback?.Travel(Animation.Broken.Name());
                }
                else
                {
                    if (!result.Props.HasFlag(ValueProp.SkipHurtAnim)&&result.Props.IsCardOrMonsterMove_())
                    {
                        if (result.UnblockedDamage>target.CurrentHp*0.25f)
                        {
                            playback?.Travel(Animation.DamageHeavy.Name());
                        }
                        else
                        {
                            playback?.Travel(Animation.DamageLight.Name());
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!wasRemovalPrevented&&creature.IsPlayer&&creature.Player?.Character is HakureiReimu c)
        {
            creature.Player.RunAnimation(Animation.Dead);
        }
        return Task.CompletedTask;
    }
    
    public enum Animation
    {
        None,
        AttackCloseHeavy,
        AttackCloseLight,
        AttackCloseRound,
        AttackDashHeavy,
        AttackDashLight,
        Block,
        Broken,
        DamageHeavy,
        DamageLight,
        Dead,
        Guard,
        Idle,
        ShotA,
        ShotB,
        ShotC,
        ShotD,
        ShotLoop,
        SpellFastA,
        SpellLongA,
        SpellLongB,
        StartBattle,
        Stun,
        StunEnd,
        Win,
    }
}