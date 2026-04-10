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
        return Path.Join(MainFile.ModId, "images", path);
    }

    public static string CardImagePath(this string path) {
        return Path.Join(MainFile.ModId, "images", "cards", path);
    }
    public static string CardItemPath(this string path) {
        return Path.Join(MainFile.ModId, "images", "card_item", path);
    }
    public static string BigCardImagePath(this string path) {
        return Path.Join(MainFile.ModId, "images", "cards", path);
    }

    public static string PowerImagePath(this string path) {
        return Path.Join(MainFile.ModId, "images", "powers", path);
    }

    public static string BigPowerImagePath(this string path) {
        return Path.Join(MainFile.ModId, "images", "powers", "big", path);
    }

    public static string RelicImagePath(this string path) {
        return Path.Join(MainFile.ModId, "images", "relics", path);
    }

    public static string BigRelicImagePath(this string path) {
        return Path.Join(MainFile.ModId, "images", "relics", path);
    }
    public static string CharacterPath(this string path) {
        return Path.Join(MainFile.ModId, "images", "char", path);
    }
    public static string CharacterUiPath(this string path) {
        return Path.Join(MainFile.ModId, "images", "charui", path);
    }
    public static string EffectsPath(this string path) {
        return Path.Join(MainFile.ModId, "images", "effects", path);
    }
    public static string ShaderPath(this string path) {
        return Path.Join(MainFile.ModId, "shader", path);
    }
    public static string ScenePath(this string path) {
        return Path.Join(MainFile.ModId, "scenes", path);
    }

}