using System;
using System.Collections;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CrosshairOverlay;

[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
public class Mod : BaseUnityPlugin
{
    public const string pluginGuid = "kestrel.straftat.crosshairoverlay";
    public const string pluginName = "Crosshair Overlay";
    public const string pluginVersion = "1.1.1";
    
    public static Mod Instance { get; private set; }
    internal static new ManualLogSource Logger;

    public static string PluginPath { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
    static string imagePath;
    static Sprite overlaySprite;
    static GameObject root;
    static Image imageComponent;
    static RectTransform transformComponent;
    static bool queueImageReload;
    static bool imageLoaded;
    
    static string loadBearingColonThree = ":3";
    private void Awake() {
        if (loadBearingColonThree != ":3") Application.Quit();
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        Instance = this;
        Logger = base.Logger;
        
        Configs.BindTo(Config);

        Config.SettingChanged += OnConfigChanged;
        imagePath = Path.Combine(PluginPath, Configs.filename.Value);
        
        if (!File.Exists(imagePath)) 
            Logger.LogWarning($"No image file found at {imagePath}");
        else 
            StartCoroutine(LoadCrosshairImage());
        
        new Harmony(pluginGuid).PatchAll();
        Logger.LogInfo("Hiiiiiiiiiiii :3");
    }

    void OnConfigChanged(object sender, SettingChangedEventArgs e) {
        queueImageReload = true;
        if (e.ChangedSetting != Configs.filename) return;
        imagePath = Path.Combine(PluginPath, Configs.filename.Value);
        if (!File.Exists(imagePath)) {
            Logger.LogWarning($"No image file found at {imagePath}");
            return;
        }

        StartCoroutine(LoadCrosshairImage());
    }

    static IEnumerator LoadCrosshairImage() {
        imageLoaded = false;
        using var uwr = UnityWebRequestTexture.GetTexture("file:///" + Path.Combine(PluginPath, Configs.filename.Value));
        
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success) {
            // Panic!!!!!1!
            Logger.LogError("Error while loading overlay image ~ " + uwr.error + $"(the file at {imagePath} is probably invalid.)");
        } else {
            Logger.LogInfo("Found and loaded crosshair overlay!");
            var texture = DownloadHandlerTexture.GetContent(uwr);
            overlaySprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            imageLoaded = true;
        }
    }

    static void SetupCrosshair(Transform uiRoot = null) {
        root = new GameObject("Crosshair Overlay");
        transformComponent = root.AddComponent<RectTransform>();
        var group = root.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        imageComponent = root.AddComponent<Image>();
        if (uiRoot) root.transform.SetParent(uiRoot, false);
    }

    static void ReloadCrosshair() {
        transformComponent.sizeDelta = new Vector2(Configs.width.Value/2f, Configs.height.Value/2f);
        transformComponent.anchoredPosition = new Vector2(Configs.offsetX.Value, Configs.offsetY.Value);
        imageComponent.color = new Color32(Configs.tintR.Value, Configs.tintG.Value, Configs.tintB.Value, Configs.tintA.Value);
        overlaySprite.texture.filterMode = Configs.filteringMode.Value;
        imageComponent.sprite = overlaySprite;
        if (!Configs.overlay.Value) root.transform.SetAsFirstSibling();
        else root.transform.SetAsLastSibling();
    }
    
    [HarmonyPatch(typeof(Crosshair))]
    public static class CrosshairPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void SetupCrosshairOverlay(Crosshair __instance) {
            if (Configs.hideDefault.Value) __instance.transform.localScale = Vector3.zero;
            SetupCrosshair(__instance.transform.parent);
            if (imageLoaded) ReloadCrosshair();
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static void UpdateOverlayVisibility(ref Crosshair __instance) {
            if (queueImageReload && imageLoaded) {
                ReloadCrosshair();
                queueImageReload = false;
            }
            
            if (Configs.hideWhenScoped.Value && __instance.player is not null && __instance.canScopeAim && __instance.player.isAiming) {
                root.SetActive(false);
            }
            else if (Configs.hideInMenus.Value && SceneManager.GetActiveScene().name == "MainMenu") {
                root.SetActive(false);
            }
            else root.SetActive(true);
        }
    }
}

