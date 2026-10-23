#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public static class DarkHuntMainMenuBuilder
{
    private const string Root = "Assets/UI";
    private const string SceneFolder = Root + "/Scenes";
    private const string PrefabFolder = Root + "/Prefabs";
    private const string AnimationFolder = Root + "/Animations";
    private const string AudioFolder = Root + "/Audio";
    private const string ScriptFolder = Root + "/Scripts";

    [MenuItem("Dark Hunt UI/Build Main Menu Scene")]
    public static void BuildMainMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder("Assets", "UI");
        EnsureFolder(Root, "Scenes");
        EnsureFolder(Root, "Prefabs");
        EnsureFolder(Root, "Animations");
        EnsureFolder(Root, "Audio");
        EnsureFolder(Root, "Scripts");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        var canvasGO = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CreateImage("Background", canvasGO.transform, new Color32(5, 8, 15, 255), true);
        CreateImage("LeftAtmosphere", canvasGO.transform, new Color32(10, 24, 43, 115), false, new Vector2(0.0f, 0.0f), new Vector2(0.62f, 1.0f));
        CreateImage("RightShade", canvasGO.transform, new Color32(0, 0, 0, 90), false, new Vector2(0.58f, 0.0f), new Vector2(1.0f, 1.0f));
        CreateImage("TopRule", canvasGO.transform, new Color32(104, 174, 207, 110), false, new Vector2(0.08f, 0.90f), new Vector2(0.92f, 0.902f));
        CreateImage("BottomRule", canvasGO.transform, new Color32(104, 174, 207, 75), false, new Vector2(0.08f, 0.10f), new Vector2(0.92f, 0.102f));

        TMP_Text eyebrow = CreateText("Eyebrow", canvasGO.transform, "A HEAD-TRACKED HORROR EXPERIENCE", 18, new Color32(126, 169, 190, 255), TextAlignmentOptions.Center);
        SetAnchors(eyebrow.rectTransform, new Vector2(0.18f, 0.75f), new Vector2(0.82f, 0.80f));

        TMP_Text title = CreateText("GameTitle", canvasGO.transform, "DARK HUNT", 104, new Color32(238, 243, 248, 255), TextAlignmentOptions.Center);
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 8;
        SetAnchors(title.rectTransform, new Vector2(0.12f, 0.57f), new Vector2(0.88f, 0.75f));

        TMP_Text subtitle = CreateText("Subtitle", canvasGO.transform, "THE DARKNESS HIDES EVERYTHING", 22, new Color32(160, 177, 190, 255), TextAlignmentOptions.Center);
        subtitle.characterSpacing = 4;
        SetAnchors(subtitle.rectTransform, new Vector2(0.20f, 0.51f), new Vector2(0.80f, 0.57f));

        RectTransform buttonGroup = new GameObject("MenuButtons", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
        buttonGroup.SetParent(canvasGO.transform, false);
        SetAnchors(buttonGroup, new Vector2(0.34f, 0.22f), new Vector2(0.66f, 0.48f));
        buttonGroup.anchorMin = new Vector2(0.34f, 0.22f);
        buttonGroup.anchorMax = new Vector2(0.66f, 0.48f);
        buttonGroup.offsetMin = Vector2.zero;
        buttonGroup.offsetMax = Vector2.zero;
        VerticalLayoutGroup layout = buttonGroup.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = buttonGroup.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreateButton("PlayButton", "PLAY", buttonGroup, true);
        CreateButton("LevelSelectButton", "LEVEL SELECT", buttonGroup, false);
        CreateButton("SettingsButton", "SETTINGS", buttonGroup, false);
        CreateButton("QuitButton", "QUIT", buttonGroup, false);

        TMP_Text footer = CreateText("Footer", canvasGO.transform, "LOOK CLOSER. TRUST NOTHING.", 14, new Color32(98, 119, 135, 255), TextAlignmentOptions.Center);
        footer.characterSpacing = 3;
        SetAnchors(footer.rectTransform, new Vector2(0.20f, 0.035f), new Vector2(0.80f, 0.075f));

        if (Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        GameObject effects = new GameObject("MainMenuEffects");
        effects.AddComponent<DarkHuntMainMenuEffects>();

        string scenePath = SceneFolder + "/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
        bool alreadyListed = false;
        foreach (var entry in current) if (entry.path == scenePath) alreadyListed = true;
        if (!alreadyListed)
        {
            var updated = new EditorBuildSettingsScene[current.Length + 1];
            current.CopyTo(updated, 0);
            updated[current.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("DARK HUNT UI", "MainMenu scene created at " + scenePath + "\n\nOpen Game view to preview it. Navigation buttons are placeholders except Quit.", "Let's go");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }

    private static GameObject CreateImage(string name, Transform parent, Color color, bool stretch, Vector2 min = default, Vector2 max = default)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        if (stretch) SetAnchors(rt, Vector2.zero, Vector2.one);
        else SetAnchors(rt, min, max);
        return go;
    }

    private static TMP_Text CreateText(string name, Transform parent, string value, float size, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TMP_Text text = go.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
        return text;
    }

    private static void CreateButton(string name, string label, Transform parent, bool primary)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(DarkHuntMenuButton));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = primary ? new Color32(20, 43, 60, 235) : new Color32(10, 16, 24, 210);
        Button button = go.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = primary ? new Color32(30, 60, 78, 255) : new Color32(18, 28, 38, 255);
        colors.highlightedColor = new Color32(47, 100, 126, 255);
        colors.pressedColor = new Color32(12, 27, 38, 255);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.16f;
        button.colors = colors;
        LayoutElement element = go.GetComponent<LayoutElement>();
        element.preferredHeight = primary ? 66 : 56;
        element.minHeight = primary ? 66 : 56;
        TMP_Text text = CreateText("Label", go.transform, label, primary ? 24 : 20, new Color32(235, 241, 246, 255), TextAlignmentOptions.Center);
        text.characterSpacing = 3;
        SetAnchors(text.rectTransform, Vector2.zero, Vector2.one);
        DarkHuntMenuButton behavior = go.GetComponent<DarkHuntMenuButton>();
        behavior.button = button;
        behavior.buttonLabel = text;
        behavior.isPrimary = primary;
        if (label == "QUIT") button.onClick.AddListener(() => { if (Application.isPlaying) Application.Quit(); else Debug.Log("Quit button preview: Application.Quit only works in a build."); });
        else button.onClick.AddListener(() => Debug.Log(label + " selected. Navigation will be connected when the other UI scenes are ready."));
    }

    private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }
}
#endif
