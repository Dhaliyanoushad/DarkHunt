using UnityEngine;

// Static drawing helpers used only by chapter selection and successful completion screens.
// No audio, animation, navigation, gameplay state, or persistent data.
public static class DarkHuntChapterTheme
{
    public static readonly Color Black = new Color32(8, 6, 8, 255);
    public static readonly Color Blood = new Color32(139, 16, 32, 255);
    public static readonly Color Crimson = new Color32(58, 11, 19, 255);
    public static readonly Color Bone = new Color32(221, 212, 200, 255);
    public static readonly Color Ash = new Color32(150, 136, 133, 255);
    static readonly Color Metal = new Color32(74, 53, 55, 255);
    static Texture2D grain;
    static Font serif;
    static bool fontLoaded;
    static GUIStyle copy, display, control;
    static Matrix4x4 previousMatrix;

    public static string ChapterName(int level)
    {
        switch (level)
        {
            case 1: return "THE AWAKENING";
            case 2: return "INTO THE DARKNESS";
            case 3: return "NO ESCAPE";
            default: return "DARKHUNT";
        }
    }

    static void Prepare()
    {
        if (copy != null) return;
        if (!fontLoaded)
        {
            // Use a locally available serif; the bundled UI font remains the fallback.
            if (Font.GetOSInstalledFontNames().Length > 0)
                serif = Font.CreateDynamicFontFromOSFont(new[] { "Georgia", "Baskerville", "Times New Roman" }, 48);
            fontLoaded = true;
        }
        copy = DarkHuntUITheme.Style(16);
        display = DarkHuntUITheme.Style(42, bold: true);
        if (serif != null) display.font = serif;
        control = new GUIStyle(GUI.skin.button)
        {
            padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0),
            alignment = TextAnchor.MiddleCenter, fontSize = 15, fontStyle = FontStyle.Bold
        };
        control.normal.background = control.hover.background = control.active.background =
            control.focused.background = null;
        control.onNormal.background = control.onHover.background = control.onActive.background =
            control.onFocused.background = null;
        grain = new Texture2D(128, 128, TextureFormat.RGBA32, false)
        {
            name = "Chapter UI static patina", hideFlags = HideFlags.HideAndDontSave,
            wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear
        };
        var pixels = new Color32[128 * 128];
        var random = new System.Random(7319);
        for (int i = 0; i < pixels.Length; i++)
        {
            byte shade = (byte)random.Next(60, 190);
            pixels[i] = new Color32(shade, shade, shade, (byte)random.Next(6, 27));
        }
        grain.SetPixels32(pixels);
        grain.Apply(false, true);
    }

    // Fit a common artboard inside every viewport, including 4:3 and ultrawide.
    // Only these two screens use this matrix; the gameplay HUD is unaffected.
    public static void Begin()
    {
        Prepare();
        previousMatrix = GUI.matrix;
        DarkHuntUITheme.Fill(new Rect(0, 0, Screen.width, Screen.height), Black);
        float scale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
        GUI.matrix = previousMatrix * Matrix4x4.TRS(
            new Vector3((Screen.width - 1600 * scale) / 2, (Screen.height - 900 * scale) / 2, 0),
            Quaternion.identity, new Vector3(scale, scale, 1));
        Patina(new Rect(0, 0, 1600, 900), 0.3f);
        Fill(new Rect(80, 68, 1, 764), Crimson);
        Fill(new Rect(1519, 68, 1, 764), Crimson);
        Fill(new Rect(80, 68, 1440, 1), Crimson);
        Fill(new Rect(80, 831, 1440, 1), Crimson);
    }

    public static void End() => GUI.matrix = previousMatrix;
    public static void Fill(Rect r, Color color) => DarkHuntUITheme.Fill(r, color);

    static void Patina(Rect r, float opacity)
    {
        Color old = GUI.color;
        GUI.color = new Color(1, 1, 1, opacity);
        GUI.DrawTextureWithTexCoords(r, grain, new Rect(0, 0, r.width / 128, r.height / 128));
        GUI.color = old;
    }

    public static void Text(Rect r, string value, int size = 16, bool heading = false,
        Color? color = null, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        GUIStyle style = heading ? display : copy;
        style.fontSize = size;
        style.alignment = alignment;
        DarkHuntUITheme.Label(r, value, style, color ?? Bone, shadow: heading);
    }

    public static void Title(Rect r, string value, int size)
    {
        Text(r, value, size, true);
        // Broken hairlines give display lettering a worn print finish, without animation.
        for (int i = 0; i < 7; i++)
        {
            float x = r.x + r.width * (0.2f + i * 0.095f);
            Fill(new Rect(x, r.center.y + (i % 3 - 1) * size * 0.18f, 13 + i % 4 * 4, 1),
                new Color(8 / 255f, 6 / 255f, 8 / 255f, 0.55f));
        }
    }

    public static void Frame(Rect r, bool active = false)
    {
        DarkHuntUITheme.Frame(r, active ? Blood : Metal);
        Rect inner = new Rect(r.x + 5, r.y + 5, r.width - 10, r.height - 10);
        DarkHuntUITheme.Frame(inner, Crimson);
        for (int i = 0; i < 4; i++)
        {
            float x = i % 2 == 0 ? r.x : r.xMax - 22;
            float y = i < 2 ? r.y : r.yMax - 2;
            Fill(new Rect(x, y, 22, 2), active ? Blood : Ash * 0.6f);
        }
        // Fixed scratches/broken metallic seams, intentionally quiet around text.
        for (int i = 0; i < 8; i++)
        {
            float y = r.y + 23 + i * (r.height - 46) / 8;
            Fill(new Rect(r.x + (i % 3), y, 2, 4 + i % 4), Black);
            Fill(new Rect(r.xMax - 4, y + 7, 3, 1), Metal);
        }
    }

    public static void Panel(Rect r)
    {
        Fill(new Rect(r.x + 7, r.y + 9, r.width, r.height), new Color(0, 0, 0, 0.65f));
        Fill(r, new Color32(17, 11, 14, 255));
        Patina(r, 0.8f);
        Frame(r);
    }

    public static void Doorway(Rect r, bool unlocked)
    {
        Fill(r, new Color32(15, 10, 13, 255));
        Rect door = new Rect(r.x + 58, r.y + 38, r.width - 116, r.height - 66);
        Fill(new Rect(door.x - 10, door.y - 10, door.width + 20, door.height + 10), Crimson);
        Fill(door, Black);
        DarkHuntUITheme.Frame(door, new Color32(48, 26, 31, 255));
        Fill(new Rect(door.x + 12, door.y + 10, door.width - 24, door.height - 10), new Color32(11, 7, 10, 255));
        Fill(new Rect(door.center.x + 4, door.y + 14, 2, door.height - 14), unlocked ? Blood * 0.65f : Crimson);
        Fill(new Rect(door.xMax - 24, door.y + door.height * 0.57f, 4, 14), Metal);
        for (int i = 0; i < 11; i++)
            Fill(new Rect(door.x + 15 + i * (door.width - 30) / 11, door.y + 19 + i % 4 * 23, 1, 18 + i % 3 * 13), new Color(0.5f, 0.33f, 0.32f, 0.09f));
        Patina(r, 0.7f);
        if (!unlocked) Fill(r, new Color(0, 0, 0, 0.48f));
        Frame(r, unlocked);
    }

    public static void Lock(Vector2 center)
    {
        DarkHuntUITheme.Frame(new Rect(center.x - 8, center.y - 13, 16, 16), Ash);
        Fill(new Rect(center.x - 12, center.y - 3, 24, 18), Crimson);
        DarkHuntUITheme.Frame(new Rect(center.x - 12, center.y - 3, 24, 18), Ash);
        Fill(new Rect(center.x - 1, center.y + 3, 2, 6), Bone);
    }

    public static void Emblem(Vector2 center)
    {
        // An open threshold crossed by three ritual marks; no imported icon/font needed.
        DarkHuntUITheme.Frame(new Rect(center.x - 18, center.y - 25, 36, 50), Blood);
        DarkHuntUITheme.Frame(new Rect(center.x - 12, center.y - 19, 24, 44), Metal);
        for (int i = -1; i <= 1; i++) Fill(new Rect(center.x + i * 8 - 1, center.y - 11, 2, 23), Bone);
        Fill(new Rect(center.x - 46, center.y + 1, 18, 1), Blood);
        Fill(new Rect(center.x + 28, center.y + 1, 18, 1), Blood);
    }

    public static bool Button(Rect r, string value, bool primary = false, bool enabled = true)
    {
        bool usable = GUI.enabled && enabled;
        bool hover = usable && r.Contains(Event.current.mousePosition);
        Fill(r, !usable ? new Color32(18, 13, 16, 255) : primary ? (hover ? Blood : Crimson) : (hover ? Crimson : Black));
        Frame(r, usable && (hover || primary));
        control.normal.textColor = control.hover.textColor = control.active.textColor =
            control.focused.textColor = usable ? Bone : Ash;
        bool old = GUI.enabled;
        GUI.enabled = usable;
        bool clicked = GUI.Button(r, value, control);
        GUI.enabled = old;
        if (!usable) Text(r, value, 15, color: Ash);
        return clicked;
    }
}
