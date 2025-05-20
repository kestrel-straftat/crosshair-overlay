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

// Hii! if you're here wondering why <feature> is broken: i can't update this mod!
// i made the unfortunate decision while designing it to store crosshair image files
// in the mod's install directory. this had the unforseen side effect of making mod managers
// remove people's custom crosshairs whenever they updated the mod. Which isnt ideal.,,,

// by all means have a look at the code if you're trying to learn modding; i think it's
// reasonably neat and tidy at this stage. Maybe dont make the same mistake i did though
// with regards to locations for storing user's files though. use BepInEx/config/ or something.

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; }
    internal static new ManualLogSource Logger;

    public static string PluginPath { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
    
    private static string m_imagePath;
    private static Sprite m_overlaySprite;
    private static GameObject m_root;
    private static Image m_image;
    private static RectTransform m_rectTransform;
    private static bool m_queueImageReload;
    private static bool m_imageLoaded;
    
    static string loadBearingColonThree = ":3";
    private void Awake() {
        if (loadBearingColonThree != ":3") Application.Quit();
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        Instance = this;
        Logger = base.Logger;
        Configs.BindTo(Config);

        Config.SettingChanged += OnConfigChanged;
        m_imagePath = Path.Combine(PluginPath, Configs.filename.Value);
        
        if (!File.Exists(m_imagePath)) 
            Logger.LogWarning($"No image file found at {m_imagePath}");
        else 
            StartCoroutine(LoadCrosshairImage());
        
        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
        Logger.LogInfo("Hiiiiiiiiiiii :3");
    }

    private void OnConfigChanged(object sender, SettingChangedEventArgs e) {
        m_queueImageReload = true;
        if (e.ChangedSetting != Configs.filename) return;
        m_imagePath = Path.Combine(PluginPath, Configs.filename.Value);
        if (!File.Exists(m_imagePath)) {
            Logger.LogWarning($"No image file found at {m_imagePath}");
            return;
        }

        StartCoroutine(LoadCrosshairImage());
    }

    static IEnumerator LoadCrosshairImage() {
        m_imageLoaded = false;
        using var uwr = UnityWebRequestTexture.GetTexture("file:///" + Path.Combine(PluginPath, Configs.filename.Value));
        
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success) {
            // Panic!!!!!1!
            Logger.LogError("Error while loading overlay image ~ " + uwr.error + $"(the file at {m_imagePath} is probably invalid.)");
        } else {
            Logger.LogInfo("Found and loaded crosshair overlay!");
            var texture = DownloadHandlerTexture.GetContent(uwr);
            m_overlaySprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            m_imageLoaded = true;
        }
    }

    static void SetupCrosshair(Transform uiRoot = null) {
        m_root = new GameObject("Crosshair Overlay");
        m_rectTransform = m_root.AddComponent<RectTransform>();
        var group = m_root.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        m_image = m_root.AddComponent<Image>();
        if (uiRoot) m_root.transform.SetParent(uiRoot, false);
    }

    static void ReloadCrosshair() {
        m_rectTransform.sizeDelta = new Vector2(Configs.width.Value/2f, Configs.height.Value/2f);
        m_rectTransform.anchoredPosition = new Vector2(Configs.offsetX.Value, Configs.offsetY.Value);
        m_image.color = new Color32(Configs.tintR.Value, Configs.tintG.Value, Configs.tintB.Value, Configs.tintA.Value);
        m_overlaySprite.texture.filterMode = Configs.filteringMode.Value;
        m_image.sprite = m_overlaySprite;
        if (!Configs.overlay.Value) m_root.transform.SetAsFirstSibling();
        else m_root.transform.SetAsLastSibling();
    }
    
    [HarmonyPatch(typeof(Crosshair))]
    public static class CrosshairPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void SetupCrosshairOverlay(Crosshair __instance) {
            if (Configs.hideDefault.Value) __instance.transform.localScale = Vector3.zero;
            SetupCrosshair(__instance.transform.parent);
            if (m_imageLoaded) ReloadCrosshair();
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static void UpdateOverlayVisibility(ref Crosshair __instance) {
            if (m_queueImageReload && m_imageLoaded) {
                ReloadCrosshair();
                m_queueImageReload = false;
            }
            
            if (Configs.hideWhenScoped.Value && __instance.player is not null && __instance.canScopeAim && __instance.player.isAiming) {
                m_root.SetActive(false);
            }
            else if (Configs.hideInMenus.Value && SceneManager.GetActiveScene().name == "MainMenu") {
                m_root.SetActive(false);
            }
            else m_root.SetActive(true);
        }
    }
}

