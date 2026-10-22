using UnityEngine;

// Minimal HUD anchored to the Game view. No Canvas or input-catching gameplay controls.
[DisallowMultipleComponent]
[RequireComponent(typeof(GameManager))]
public sealed class DarkHuntHUD : MonoBehaviour
{
    static readonly Color Ink = new Color(0.025f, 0.038f, 0.046f, 0.94f);
    static readonly Color Line = new Color(0.20f, 0.27f, 0.29f, 0.8f);
    static readonly Color Muted = new Color(0.58f, 0.66f, 0.68f);
    static readonly Color Paper = new Color(0.91f, 0.94f, 0.92f);
    static readonly Color Accent = new Color(0.66f, 0.77f, 0.73f);
    static readonly Color Red = new Color(1f, 0.34f, 0.30f);
    GameManager game;
    GUIStyle text;
    GUIStyle button;

    void Awake() => game = GetComponent<GameManager>();

    void Box(float x, float y, float w, float h, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
        GUI.color = old;
    }

    void Label(float x, float y, float w, float h, string value, int size, Color color,
        TextAnchor alignment = TextAnchor.MiddleLeft, bool bold = false)
    {
        text.fontSize = size;
        text.alignment = alignment;
        text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        text.normal.textColor = new Color(0, 0, 0, 0.8f);
        GUI.Label(new Rect(x + 1, y + 1, w, h), value, text);
        text.normal.textColor = color;
        GUI.Label(new Rect(x, y, w, h), value, text);
    }

    bool Button(float x, float y, float w, string label, bool primary)
    {
        Rect rect = new Rect(x, y, w, 54);
        bool hover = rect.Contains(Event.current.mousePosition);
        Box(x, y, w, 54, primary ? (hover ? Paper : Accent) : (hover ? Line : Ink));
        if (!primary) Box(x, y + 53, w, 1, Line);
        button.normal.textColor = primary ? Ink : Paper;
        button.hover.textColor = button.normal.textColor;
        button.active.textColor = button.normal.textColor;
        return GUI.Button(rect, label, button);
    }

    void OnGUI()
    {
        if (game == null) return;
        if (text == null)
        {
            text = new GUIStyle(GUI.skin.label) { padding = new RectOffset(0, 0, 0, 0) };
            button = new GUIStyle(text)
            { fontSize = 18, alignment = TextAnchor.MiddleCenter };
        }
        if (game.state == GameManager.State.Playing) DrawPlaying();
        else DrawResult();
    }

    void DrawPlaying()
    {
        float width = Screen.width;
        const float margin = 20;
        const float top = 16;
        float column = Mathf.Max(1, width / 3 - margin * 2);
        Label(margin, top, column, 36, game.levelTitle, 18, Paper);
        int seconds = Mathf.CeilToInt(game.RemainingTime);
        Label(width / 2 - 60, top, 120, 36, $"{seconds / 60:00}:{seconds % 60:00}", 24,
            seconds <= 10 ? Red : Paper, TextAnchor.MiddleCenter);
        Label(width - column - margin, top, column, 36, $"OBJECTS  {game.found} / {game.total}",
            18, Paper, TextAnchor.MiddleRight);
        Label(width - column - margin, top + 34, column, 26, game.BestTimeText,
            15, Muted, TextAnchor.MiddleRight);

        float cx = width / 2;
        float cy = Screen.height / 2;
        Box(cx - 1, cy - 1, 3, 3, PlayerInteract.Looking != null ? Accent : Muted);
        if (PlayerInteract.Looking != null)
        {
            Box(cx - 122, cy + 64, 244, 36, new Color(0, 0, 0, 0.3f));
            Label(cx - 122, cy + 64, 244, 36, "PRESS F TO COLLECT", 16, Paper, TextAnchor.MiddleCenter);
        }
        if (Time.unscaledTime - game.LastPenaltyAt < 1.5f)
            Label(cx - 140, top + 46, 280, 36, $"-{game.LastPenaltySeconds:0.#} SECONDS", 20,
                Red, TextAnchor.MiddleCenter);
    }

    void DrawResult()
    {
        bool won = game.state == GameManager.State.Won;
        bool lost = game.state == GameManager.State.GameOver;
        float cx = Screen.width / 2;
        float cy = Screen.height / 2;
        float content = Mathf.Min(760, Screen.width - 32);
        Box(0, 0, Screen.width, Screen.height, Ink);
        Label(cx - content / 2, cy - 190, content, 35, game.levelTitle, 17, Muted, TextAnchor.MiddleCenter);
        Label(cx - content / 2, cy - 145, content, 64,
            lost ? "GAME OVER" : won ? "YOU ESCAPED" : "LEVEL COMPLETE",
            36, lost ? Red : Paper, TextAnchor.MiddleCenter);
        Label(cx - content / 2, cy - 68, content, 40, won ? $"Objects collected: {game.RunObjectsFound}" :
            $"Objects collected: {game.found} / {game.total}", 20, Muted, TextAnchor.MiddleCenter);
        float buttonWidth = Mathf.Min(300, content);
        if (Button(cx - buttonWidth / 2, cy, buttonWidth,
            lost ? "RETRY ROOM" : won ? "PLAY AGAIN" : "CONTINUE", true)) game.Advance();
        if (Button(cx - buttonWidth / 2, cy + 66, buttonWidth, "RESTART FROM LEVEL 1", false))
            game.RestartRun();
        if (Button(cx - buttonWidth / 2, cy + 132, buttonWidth, "LEVEL SELECTION", false))
            game.OpenLevelSelection();
    }
}
