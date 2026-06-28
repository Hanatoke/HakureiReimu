using System;
using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using Godot;
using HakureiReimu.HakureiReimuMod.Character;
using HakureiReimu.HakureiReimuMod.Core;
using HakureiReimu.HakureiReimuMod.Extensions;
using HakureiReimu.HakureiReimuMod.Interface;
using HakureiReimu.HakureiReimuMod.Patches;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Cards;

[Pool(typeof(HakureiReimuCardPool))]
public abstract class AbstractCard(int cost, CardType type, CardRarity rarity, TargetType target,bool showInCardLibrary = true, 
    bool autoAdd = true)
    : CustomCardModel(cost, type, rarity, target,showInCardLibrary, autoAdd),
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
    //------------------------------------------------------------------------------------------------
    protected virtual string SignatureId(int index) => $"{Id.Entry}.{index}";
    protected virtual string SignatureImgPath(int index) => index > 0
        ? $"{OriginId.CardSignaturePath()}_s_p_{index}.png"
        : $"{OriginId.CardSignaturePath()}_s_p.png";
    protected virtual Vector2 SignatureImgScale(int index) => Vector2.One * 0.5f;
    protected virtual Func<LocString> SignatureName(int index)
    {
        string key = $"{Id.Entry}.signature.{index}.name";
        return LocString.Exists("cards", key) ? () => new LocString("cards", key) : null;
    }

    protected virtual Func<LocString> SignatureDescription(int index)
    {
        string key = $"{Id.Entry}.signature.{index}.description";
        return LocString.Exists("cards", key) ? () => new LocString("cards", key) : null;
    }

    public virtual IEnumerable<(string,string,Vector2,Func<LocString>,Func<LocString>)> SignatureInfos
    {
        get
        {
            int index = 0;
            string s;
            while (ResourceLoader.Exists(s=SignatureImgPath(index)))
            {
                yield return (SignatureId(index),s,SignatureImgScale(index),SignatureName(index),SignatureDescription(index));
                index++;
            }
        }
    }
    //---------------------------------------------------------------------------------------------
    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190
    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{OriginId.CardImagePath()}_p.png";
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
    [CustomEnum,KeywordProperties(AutoKeywordPosition.None)]
    public static CardKeyword FreeCounter;
    [CustomEnum,KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword IgnoreDefense;

    public static readonly ValueProp IgnoreDefenseProps = ValueProp.Unblockable |
                                                          DamagePropsPatch.IgnoreDamageImmunity |
                                                          DamagePropsPatch.IgnoreDamageResponse;
    

    public void RunAnimation(Character.HakureiReimu.Animation animation)
    {
        if (Owner.Character is Character.HakureiReimu)
        {
            Character.HakureiReimu.RunAnimation(Owner, animation);
        }
    }
    
    public virtual void OnReload(NCard card, List<Action<NCard>> recoverAction, List<Godot.Node> needRemove)
    {
        Traverse traverse = Traverse.Create(card);
        ModifyBackground(card, traverse, recoverAction, needRemove);
        ModifyEnergy(card, traverse, recoverAction, needRemove);
        ModifyTitle(card, traverse, recoverAction, needRemove);
    }
    
    protected virtual void ModifyBackground(NCard card,Traverse traverse,List<Action<NCard>> recoverAction,List<Godot.Node> needRemove)
    {
        TextureRect control = traverse.Field<TextureRect>("_frame").Value;
        Vector2 originSize = control.Size;
        control.Size = new Vector2(Size, Size);
        control.Position = new Vector2(-Size/2, -Size/2);
        recoverAction.Add(c =>
        {
            if (!GodotObject.IsInstanceValid(control))return;
            control.Size = originSize;
            control.PivotOffset = control.Size / 2;
            control.Position=-control.Size/2;
        });
        
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
        
        needRemove.Add(border);
        needRemove.Add(cover);
    }
    
    public static readonly Random EnergyOffsetRandom=new();
    
    protected virtual void ModifyEnergy(NCard card, Traverse traverse,List<Action<NCard>> recoverAction,List<Godot.Node> needRemove)
    {
        TextureRect energyIcon = traverse.Field<TextureRect>("_energyIcon").Value;
        Texture2D originTexture = energyIcon.Texture;
        energyIcon.Texture=PreloadManager.Cache.GetTexture2D("energy.png".CardItemPath());
        Shader shader = GD.Load<Shader>("Energy.gdshader".ShaderPath());
        ShaderMaterial material = new();
        material.Shader = shader;
        material.SetShaderParameter("offset",(float)EnergyOffsetRandom.NextDouble()*7);
        if (ModConfig.UseStaticEnergyIcon)
        {
            material.SetShaderParameter("speed",0);
        }

        Material originMaterial = energyIcon.Material;
        energyIcon.Material=material;
        Vector2 originScale = energyIcon.Scale;
        energyIcon.Scale = new Vector2(EnergyScale,EnergyScale);
        
        recoverAction.Add(c =>
        {
            if (!GodotObject.IsInstanceValid(energyIcon))return;
            energyIcon.Scale = originScale;
            energyIcon.Material = GodotObject.IsInstanceValid(originMaterial) ? originMaterial : null;
            energyIcon.Texture = GodotObject.IsInstanceValid(originTexture) ? originTexture : null;
        });
    }

    protected virtual void ModifyTitle(NCard card, Traverse traverse,List<Action<NCard>> recoverAction,List<Godot.Node> needRemove)
    {
        if (!LocString.IsNullOrWhitespace(Sign)&&Sign.Exists())
        {
            Label sign=PreloadManager.Cache.GetScene(SignPath).Instantiate<Label>();
            MegaLabel title=traverse.Field<MegaLabel>("_titleLabel").Value;
            title.AddChildSafely(sign);
            sign.AddThemeFontOverride("font",title.GetThemeFont("font"));
            sign.SetText(Sign.GetFormattedText());
            sign.Position = new Vector2(0, -25); 
            
            needRemove.Add(sign);
        }
    }
}