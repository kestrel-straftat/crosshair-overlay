using BepInEx.Configuration;
using UnityEngine;

namespace CrosshairOverlay;

public static class Configs
{
    public static ConfigEntry<FilterMode> filteringMode;
    public static ConfigEntry<float> width;
    public static ConfigEntry<float> height;
    public static ConfigEntry<float> offsetX;
    public static ConfigEntry<float> offsetY;
    public static ConfigEntry<bool> overlay;
    public static ConfigEntry<bool> hideDefault;
    public static ConfigEntry<bool> hideInMenus;
    public static ConfigEntry<bool> hideWhenScoped;
    public static ConfigEntry<string> filename;
    
    public static ConfigEntry<byte> tintR;
    public static ConfigEntry<byte> tintG;
    public static ConfigEntry<byte> tintB;
    public static ConfigEntry<byte> tintA;

    public static void BindTo(ConfigFile config) {
        filename = config.Bind("General",
            "File name",
            "default-crosshair.png",
            $"The name of the image file to load. png and jpeg formats are supported, and the file should be placed in the mod's directory. ({Plugin.PluginPath})"
        );
        overlay = config.Bind(
            "General",
            "Overlay",
            true,
            "Wether the crosshair image should be on top of the UI. If disabled, the image will be under the UI. (NOTE: this does not include the 3D version of the player HUD~ the crosshair will always be above it.)"
        );
        filteringMode = config.Bind(
            "General",
            "Image Filter Mode",
            FilterMode.Point,
            "The texture filtering mode to use for the crosshair image."
        );
        hideDefault = config.Bind(
            "General",
            "Hide default crosshair",
            true,
            "Whether to hide the default crosshair~ this option uses the behaviour of ctrl+k, so the crosshair can be reenabled at any time."
        );
        hideInMenus = config.Bind(
            "General",
            "Hide in menu",
            true,
            "Whether to hide the custom crosshair when in the main menu."
        );
        hideWhenScoped = config.Bind(
            "General",
            "Hide when scoped",
            true,
            "Whether to hide the custom crosshair when aiming with a scoped weapon."
        );
        
        width = config.Bind("Style.Size", "Width", 30f, "The width of the crosshair image.");
        height = config.Bind("Style.Size", "Height", 30f, "The height of the crosshair image.");
        offsetX = config.Bind("Style.Offset", "X Offset", 0f, "The X offset of the crosshair image.");
        offsetY = config.Bind("Style.Offset", "Y Offset", 0f, "The Y offset of the crosshair image.");
        
        tintR = config.Bind("Style.Color", "1 Tint R", (byte)255, new ConfigDescription("The red channel tint of the crosshair image.", new AcceptableValueRange<byte>(0, 255)));
        tintG = config.Bind("Style.Color", "2 Tint G", (byte)255, new ConfigDescription("The green channel tint of the crosshair image.", new AcceptableValueRange<byte>(0, 255)));
        tintB = config.Bind("Style.Color", "3 Tint B", (byte)255, new ConfigDescription("The blue channel tint of the crosshair image.", new AcceptableValueRange<byte>(0, 255)));
        tintA = config.Bind("Style.Color", "4 Tint A", (byte)255, new ConfigDescription("The alpha channel tint of the crosshair image.", new AcceptableValueRange<byte>(0, 255)));
    }
}