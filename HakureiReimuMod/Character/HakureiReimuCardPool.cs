using BaseLib.Abstracts;
using Godot;
using HakureiReimu.HakureiReimuMod.Extensions;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HakureiReimu.HakureiReimuMod.Character;

public class HakureiReimuCardPool : CustomCardPoolModel {
    public override string Title => HakureiReimu.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "energy.png".CardItemPath();
    public override string TextEnergyIconPath => "text_energy.png".CharacterUiPath();

    public override float H => 1f; //Hue; changes the color.
    public override float S => 1f; //Saturation
    public override float V => 1f; //Brightness

    public override Texture2D CustomFrame(CustomCardModel card)
    {
        Texture2D texture;
        switch (card.Type)
        {
            case CardType.Attack:
                texture =  PreloadManager.Cache.GetTexture2D("bg_attack_pink.png".CardItemPath());
                break;
            case CardType.Power:
                texture =  PreloadManager.Cache.GetTexture2D("bg_power_pink.png".CardItemPath());
                break;
            default:
                texture =  PreloadManager.Cache.GetTexture2D("bg_skill_pink.png".CardItemPath());
                break;
        }
        return texture;
    }

    //Color of small card icons
    public override Color DeckEntryCardColor => new("ffffff");

    public override bool IsColorless => false;
}