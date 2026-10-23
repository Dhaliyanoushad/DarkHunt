using UnityEngine;

// Drawing only: shared typography, colours, borders and hover states. No game state.
public static class DarkHuntUITheme
{
    public static readonly Color Background = new Color32(9, 8, 10, 255);
    public static readonly Color Overlay = new Color32(9, 8, 10, 248);
    public static readonly Color Surface = new Color32(23, 20, 23, 255);
    public static readonly Color Border = new Color32(66, 48, 54, 255);
    public static readonly Color Text = new Color32(235, 229, 225, 255);
    public static readonly Color Muted = new Color32(179, 165, 168, 255);
    public static readonly Color Disabled = new Color32(156, 143, 148, 255);
    public static readonly Color Crimson = new Color32(106, 35, 49, 255);
    public static readonly Color Highlight = new Color32(156, 71, 86, 255);
    public static readonly Color Warning = new Color32(222, 120, 131, 255);

    public static GUIStyle Style(int size, TextAnchor alignment = TextAnchor.MiddleCenter,
        bool bold = false, bool wrap = false)
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = size, alignment = alignment, wordWrap = wrap,
            fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
            padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0)
        };
    }

    public static void Fill(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    public static void Frame(Rect rect, Color color)
    {
        Fill(new Rect(rect.x, rect.y, rect.width, 1), color);
        Fill(new Rect(rect.x, rect.yMax - 1, rect.width, 1), color);
        Fill(new Rect(rect.x, rect.y, 1, rect.height), color);
        Fill(new Rect(rect.xMax - 1, rect.y, 1, rect.height), color);
    }

    public static void Panel(Rect rect, bool shadow = true)
    {
        if (shadow)
        {
            Fill(new Rect(rect.x - 2, rect.y + 5, rect.width + 4, rect.height + 2), new Color(0, 0, 0, 0.14f));
            Fill(new Rect(rect.x - 1, rect.y + 3, rect.width + 2, rect.height), new Color(0, 0, 0, 0.24f));
        }
        Fill(rect, Surface);
        Frame(rect, Border);
    }

    public static void Label(Rect rect, string value, GUIStyle style, Color color, bool shadow = false)
    {
        int originalSize = style.fontSize;
        // Keep short labels inside their bounds in smaller Editor Game views.
        if (!style.wordWrap && rect.width > 0)
        {
            float measured = style.CalcSize(new GUIContent(value)).x;
            if (measured > rect.width)
                style.fontSize = Mathf.Max(10, Mathf.FloorToInt(originalSize * rect.width / measured));
        }
        if (shadow)
        {
            style.normal.textColor = new Color(0, 0, 0, 0.85f);
            GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), value, style);
        }
        style.normal.textColor = color;
        GUI.Label(rect, value, style);
        style.fontSize = originalSize;
    }

    public static bool Button(Rect rect, string value, GUIStyle style, bool primary,
        bool enabled = true, bool danger = false)
    {
        bool usable = GUI.enabled && enabled;
        bool hover = usable && rect.Contains(Event.current.mousePosition);
        Color fill = !usable ? new Color32(20, 18, 21, 255) : primary
            ? (hover ? new Color32(132, 46, 63, 255) : Crimson)
            : (hover ? new Color32(37, 28, 33, 255) : Surface);
        if (danger && usable) fill = hover ? new Color32(146, 45, 61, 255) : new Color32(116, 32, 46, 255);
        Fill(new Rect(rect.x, rect.y + 2, rect.width, rect.height), new Color(0, 0, 0, 0.20f));
        Fill(rect, fill);
        Frame(rect, hover ? Highlight : primary && usable ? new Color32(139, 55, 72, 255) : Border);
        style.normal.textColor = usable ? Text : Disabled;
        style.hover.textColor = style.active.textColor = style.focused.textColor = style.normal.textColor;
        bool wasEnabled = GUI.enabled;
        GUI.enabled = usable;
        bool clicked = GUI.Button(rect, value, style);
        GUI.enabled = wasEnabled;
        // Keep the locked label legible without enabling its button.
        if (!usable) Label(rect, value, style, Disabled);
        return clicked;
    }
}
