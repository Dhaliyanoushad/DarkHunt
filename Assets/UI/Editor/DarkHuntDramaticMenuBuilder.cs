#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public static class DarkHuntDramaticMenuBuilder
{
    private const string ArtworkPath = "Assets/UI/Artwork/DarkHunt_Dramatic_Menu.png";
    private const string MusicPath = "Assets/UI/Audio/DarkHunt_Ambient_Drone.wav";
    private const string ScenePath = "Assets/UI/Scenes/MainMenuDramatic.unity";

    [MenuItem("Dark Hunt UI/Build Dramatic Main Menu (New Scene)")]
    public static void Build()
    {
        if (!System.IO.File.Exists(ArtworkPath))
        {
            EditorUtility.DisplayDialog("DARK HUNT", "Artwork not found at:\n" + ArtworkPath + "\n\nCopy the included PNG into Assets/UI/Artwork first.", "OK");
            return;
        }

        EnsureFolder("Assets/UI/Scenes");
        EnsureFolder("Assets/UI/Audio");
        EnsureSpriteImport(ArtworkPath);

        if (System.IO.File.Exists(ScenePath) && !EditorUtility.DisplayDialog("DARK HUNT", "MainMenuDramatic already exists. Rebuild and replace that concept scene? Your existing MainMenu and Level1 will not be changed.", "Replace concept scene", "Cancel")) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenuDramatic";

        var canvasGO = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var group = canvasGO.GetComponent<CanvasGroup>();

        var bgGO = new GameObject("DramaticArtwork", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRect = bgGO.GetComponent<RectTransform>();
        Stretch(bgRect);
        var bgImage = bgGO.GetComponent<Image>();
        bgImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtworkPath);
        bgImage.preserveAspect = false;
        bgImage.raycastTarget = false;

        var tintGO = new GameObject("AtmosphericRedPulse", typeof(RectTransform), typeof(Image));
        tintGO.transform.SetParent(canvasGO.transform, false);
        var tintRect = tintGO.GetComponent<RectTransform>();
        Stretch(tintRect);
        var tint = tintGO.GetComponent<Image>();
        tint.color = new Color(0.35f, 0.005f, 0.015f, 0f);
        tint.raycastTarget = false;

        var audioGO = new GameObject("MenuAudio");
        audioGO.transform.SetParent(canvasGO.transform, false);
        var audio = audioGO.AddComponent<AudioSource>();
        audio.playOnAwake = true;
        audio.loop = true;
        audio.volume = 0.32f;
        audio.spatialBlend = 0f;
        var music = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicPath);
        if (music != null) audio.clip = music;

        var atmosphere = canvasGO.AddComponent<DarkHuntMenuAtmosphere>();
        SetPrivate(atmosphere, "artwork", bgRect);
        SetPrivate(atmosphere, "canvasGroup", group);
        SetPrivate(atmosphere, "tintOverlay", tint);

        CreateHotspot(canvasGO.transform, "PlayHotspot", new Vector2(-535f, 90f), new Vector2(520f, 82f), audio, true);
        CreateHotspot(canvasGO.transform, "LevelSelectHotspot", new Vector2(-535f, -7f), new Vector2(520f, 80f), audio, false);
        CreateHotspot(canvasGO.transform, "SettingsHotspot", new Vector2(-535f, -105f), new Vector2(520f, 80f), audio, false);
        CreateHotspot(canvasGO.transform, "QuitHotspot", new Vector2(-535f, -203f), new Vector2(520f, 80f), audio, false);

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var eventGO = new GameObject("EventSystem", typeof(EventSystem));
            // Input module for the classic Unity UI input backend.
            eventGO.AddComponent<StandaloneInputModule>();
        }

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettingsScene[] oldScenes = EditorBuildSettings.scenes;
        if (!System.Array.Exists(oldScenes, s => s.path == ScenePath))
        {
            var next = new EditorBuildSettingsScene[oldScenes.Length + 1];
            oldScenes.CopyTo(next, 0);
            next[next.Length - 1] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = next;
        }
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = canvasGO;
        EditorUtility.DisplayDialog("DARK HUNT", "Created Assets/UI/Scenes/MainMenuDramatic.unity.\n\nThe artwork and hover zones are ready. PLAY, LEVEL SELECT and SETTINGS are visual placeholders for now; QUIT exits the application. Ambient music is assigned if the included WAV imported successfully.", "Let's go");
    }

    private static void CreateHotspot(Transform parent, string name, Vector2 position, Vector2 size, AudioSource audio, bool play)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(DarkHuntMenuHotspot));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var hit = go.GetComponent<Image>();
        hit.color = new Color(1f, 1f, 1f, 0.001f);
        hit.raycastTarget = true;
        var button = go.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        var washGO = new GameObject("HoverWash", typeof(RectTransform), typeof(Image));
        washGO.transform.SetParent(go.transform, false);
        var washRect = washGO.GetComponent<RectTransform>();
        Stretch(washRect);
        var wash = washGO.GetComponent<Image>();
        wash.color = new Color(0.8f, 0f, 0.025f, 0f);
        wash.raycastTarget = false;
        var hotspot = go.GetComponent<DarkHuntMenuHotspot>();
        SetPrivate(hotspot, "hoverWash", wash);
        SetPrivate(hotspot, "audioSource", audio);
        button.onClick.AddListener(() =>
        {
            if (name == "QuitHotspot")
            {
                if (EditorApplication.isPlaying) Application.Quit();
                else EditorUtility.DisplayDialog("DARK HUNT", "Quit works in a built game. Navigation buttons will be connected when the other scenes are ready.", "OK");
            }
            else if (!play)
            {
                EditorUtility.DisplayDialog("DARK HUNT", "This button is a placeholder. We'll connect it after the other UI scenes are ready.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("DARK HUNT", "PLAY will be connected to Level1 after the menu design is approved.", "OK");
            }
        });
    }

    private static void EnsureSpriteImport(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string name = System.IO.Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void SetPrivate(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field != null) field.SetValue(target, value);
    }
}
#endif
