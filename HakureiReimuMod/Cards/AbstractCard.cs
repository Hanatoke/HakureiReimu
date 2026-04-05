using System;
using BaseLib.Abstracts;
using BaseLib.Config;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using Godot;
using HakureiReimu.HakureiReimuMod.Character;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Interface;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HakureiReimu.HakureiReimuMod.Cards;

[Pool(typeof(HakureiReimuCardPool))]
public abstract class AbstractCard(int cost, CardType type, CardRarity rarity, TargetType target) :CustomCardModel(cost, type, rarity, target),
    INCardModify
{
    public const float EnergyScale = 1.5f;
    public const float Size = 512;
    public static readonly string SignPath = "sign.tscn".ScenePath();
    public virtual Character.HakureiReimu.Animation Animation=>Character.HakureiReimu.Animation.None;
    protected string _OriginId;
    public virtual string OriginId => _OriginId ??= StringHelper.Unslugify(Id.Entry.RemovePrefix());
    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => $"{OriginId.BigCardImagePath()}_p.png";

    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190
    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{OriginId.CardImagePath()}.png";
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    protected LocString _Sign;
    public virtual LocString Sign => _Sign ??= LocString.GetIfExists("cards", this.Id.Entry + ".sign");
    
    [CustomEnum,KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Counter;
    [CustomEnum,KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Attack;
    [CustomEnum,KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Buff;
    [CustomEnum,KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Debuff;
    [CustomEnum,KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword All;
    [CustomEnum,KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword Immediate;
    
    public virtual void OnReload(NCard card)
    {
        Traverse traverse = Traverse.Create(card);
        ModifyBackground(card, traverse);
        ModifyEnergy(card, traverse);
        ModifyTitle(card, traverse);
    }

    // private static Vector2? OldSize = null;
    // private static Vector2? OldPosition = null;
    protected virtual void ModifyBackground(NCard card,Traverse traverse)
    {
        TextureRect control = traverse.Field<TextureRect>("_frame").Value;
        
        control.Size = new Vector2(Size, Size);
        control.Position = new Vector2(-Size/2, -Size/2);
        
        TextureRect border = new TextureRect();
        border.Size=new Vector2(Size,Size);
        border.ExpandMode = TextureRect.ExpandModeEnum.FitHeight;
        border.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        border.MouseFilter = Control.MouseFilterEnum.Ignore;
        border.Texture=PreloadManager.Cache.GetTexture2D("bg_border.png".CardItemPath());
        Shader shader = GD.Load<Shader>("Border.gdshader".ShaderPath());
        ShaderMaterial material = new();
        material.Shader = shader;
        border.Material = material;
        control.AddChild(border);
        
        TextureRect cover=new TextureRect();
        cover.Size=new Vector2(Size,Size);
        cover.ExpandMode = TextureRect.ExpandModeEnum.FitHeight;
        cover.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        cover.MouseFilter = Control.MouseFilterEnum.Ignore;
        cover.Texture=PreloadManager.Cache.GetTexture2D("bg_cover.png".CardItemPath());
        control.AddChild(cover);
    }
    
    public static readonly Random EnergyOffsetRandom=new();
    
    protected virtual void ModifyEnergy(NCard card, Traverse traverse)
    {
        TextureRect energyIcon = traverse.Field<TextureRect>("_energyIcon").Value;
        Shader shader = GD.Load<Shader>("Energy.gdshader".ShaderPath());
        ShaderMaterial material = new();
        material.Shader = shader;
        material.SetShaderParameter("offset",(float)EnergyOffsetRandom.NextDouble()*7);
        energyIcon.Material=material;
        energyIcon.Scale = new Vector2(EnergyScale,EnergyScale);
    }

    protected virtual void ModifyTitle(NCard card, Traverse traverse)
    {
        if (!LocString.IsNullOrWhitespace(Sign)&&Sign.Exists())
        {
            Label sign=PreloadManager.Cache.GetScene(SignPath).Instantiate<Label>();
            MegaLabel title=traverse.Field<MegaLabel>("_titleLabel").Value;
            title.AddChildSafely(sign);
            sign.AddThemeFontOverride(ThemeConstants.Label.font,title.GetThemeFont(ThemeConstants.Label.font));
            sign.SetText(Sign.GetFormattedText());
            sign.Position = new Vector2(0, -25);
        }
    }
}