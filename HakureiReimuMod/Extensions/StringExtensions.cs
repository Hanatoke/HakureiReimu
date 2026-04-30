using System;
using System.IO;

namespace HakureiReimu.HakureiReimuMod.Extensions;

//Mostly utilities to get asset paths.
public static class StringExtensions {
    public static string Name(this Enum enumValue)
    {
        return Enum.GetName(enumValue.GetType(), enumValue);
    }
    public static string ImagePath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", path);
    }
    public static string UiImagePath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images","ui", path);
    }

    public static string CardImagePath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "cards", path);
    }
    public static string CardItemPath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "card_item", path);
    }
    public static string BigCardImagePath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "cards", path);
    }

    public static string PowerImagePath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "powers", path);
    }

    public static string BigPowerImagePath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "powers", "big", path);
    }

    public static string RelicImagePath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "relics", path);
    }

    public static string BigRelicImagePath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "relics", path);
    }
    public static string PotionsImagePath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "potions", path);
    }
    public static string CharacterPath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "char", path);
    }
    public static string CharacterUiPath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "charui", path);
    }
    public static string EffectsPath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "effects", path);
    }
    public static string EnchantmentPath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "images", "enchantments", path);
    }
    public static string ShaderPath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "shader", path);
    }
    public static string ScenePath(this string path) {
        return Path.Join(HakureiReimuMain.ModId, "scenes", path);
    }

}