using UnityEngine;

// Presentation only. Gameplay values and button actions belong to the existing managers.
[DisallowMultipleComponent]
[RequireComponent(typeof(GameManager))]
public sealed class DarkHuntHUD : MonoBehaviour
{
    GameManager game;
    GUIStyle text;
    GUIStyle button;

    // Read-only presentation snapshot: GameManager remains the sole writer of scores.
    bool hadPersonalBest;
    float previousPersonalBest;

    void Awake()
    {
        game = GetComponent<GameManager>();
        hadPersonalBest = LevelProgress.TryGetBestTime(LevelProgress.LevelForScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name), out previousPersonalBest);
    }

    void Label(float x, float y, float w, float h, string value, int size, Color color,
        TextAnchor alignment = TextAnchor.MiddleLeft, bool bold = false)
    {
        text.fontSize = size;
        text.alignment = alignment;
        text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        DarkHuntUITheme.Label(new Rect(x, y, w, h), value, text, color, shadow: true);
    }

    bool Button(float x, float y, float w, string label, bool primary) =>
        DarkHuntUITheme.Button(new Rect(x, y, w, 46), label, button, primary);

    void OnGUI()
    {
        if (game == null) return;
        if (text == null)
        {
            text = DarkHuntUITheme.Style(16, TextAnchor.MiddleLeft);
            button = DarkHuntUITheme.Style(16);
        }
        if (game.state == GameManager.State.Playing) DrawPlaying();
        else DrawResult();
    }

    void DrawPlaying()
    {
        float width = Screen.width;
        const float margin = 24;
        const float top = 16;
        float column = Mathf.Max(1, width / 3 - margin * 2);
        DarkHuntUITheme.Fill(new Rect(margin, top + 7, 2, 18), DarkHuntUITheme.Highlight);
        Label(margin + 12, top, column - 12, 32, game.levelTitle, 16, DarkHuntUITheme.Text);
        int seconds = Mathf.CeilToInt(game.RemainingTime);
        Color timerColor = seconds <= 10 ? DarkHuntUITheme.Warning : DarkHuntUITheme.Text;
        Label(width / 2 - 60, top - 3, 120, 38, $"{seconds / 60:00}:{seconds % 60:00}", 26,
            timerColor, TextAnchor.MiddleCenter);
        DarkHuntUITheme.Fill(new Rect(width / 2 - 16, top + 39, 32, 1),
            seconds <= 10 ? DarkHuntUITheme.Warning : DarkHuntUITheme.Crimson);
        Label(width - column - margin, top, column, 30, $"OBJECTS  {game.found} / {game.total}",
            16, DarkHuntUITheme.Text, TextAnchor.MiddleRight);
        Label(width - column - margin, top + 32, column, 24, game.BestTimeText,
            14, DarkHuntUITheme.Muted, TextAnchor.MiddleRight);

        float cx = width / 2;
        float cy = Screen.height / 2;
        DarkHuntUITheme.Fill(new Rect(cx - 1, cy - 1, 3, 3),
            PlayerInteract.Looking != null ? DarkHuntUITheme.Warning : new Color(0.75f, 0.70f, 0.71f, 0.6f));
        if (PlayerInteract.Looking != null)
        {
            Rect prompt = new Rect(cx - 124, cy + 60, 248, 40);
            DarkHuntUITheme.Panel(prompt, shadow: false);
            DarkHuntUITheme.Fill(new Rect(prompt.x, prompt.y + 1, 2, prompt.height - 2), DarkHuntUITheme.Highlight);
            Label(prompt.x + 12, prompt.y, prompt.width - 24, prompt.height,
                "PRESS F TO COLLECT", 15, DarkHuntUITheme.Text, TextAnchor.MiddleCenter);
        }
        if (Time.unscaledTime - game.LastPenaltyAt < 1.5f)
            Label(cx - 140, top + 54, 280, 32, $"-{game.LastPenaltySeconds:0.#} SECONDS", 17,
                DarkHuntUITheme.Warning, TextAnchor.MiddleCenter);
    }

    void DrawResult()
    {
        if (game.state != GameManager.State.GameOver)
        {
            DrawVictory();
            return;
        }
        bool won = game.state == GameManager.State.Won;
        bool lost = game.state == GameManager.State.GameOver;
        float cx = Screen.width / 2;
        float cy = Screen.height / 2;
        Rect panel = new Rect(cx - Mathf.Min(480, Screen.width - 40) / 2, cy - 194,
            Mathf.Min(480, Screen.width - 40), 388);
        float content = panel.width - 64;
        DarkHuntUITheme.Fill(new Rect(0, 0, Screen.width, Screen.height), DarkHuntUITheme.Overlay);
        DarkHuntUITheme.Panel(panel);
        Label(panel.x + 32, panel.y + 26, content, 26, game.levelTitle, 14,
            DarkHuntUITheme.Muted, TextAnchor.MiddleCenter);
        Label(panel.x + 32, panel.y + 64, content, 48,
            lost ? "GAME OVER" : won ? "YOU ESCAPED" : "LEVEL COMPLETE",
            32, lost ? DarkHuntUITheme.Warning : DarkHuntUITheme.Text, TextAnchor.MiddleCenter);
        Label(panel.x + 32, panel.y + 124, content, 32, won ? $"Objects collected: {game.RunObjectsFound}" :
            $"Objects collected: {game.found} / {game.total}", 16, DarkHuntUITheme.Muted, TextAnchor.MiddleCenter);
        DarkHuntUITheme.Fill(new Rect(panel.x + 32, panel.y + 172, content, 1), DarkHuntUITheme.Border);
        float buttonWidth = Mathf.Min(320, content);
        if (Button(cx - buttonWidth / 2, panel.y + 196, buttonWidth,
            lost ? "RETRY ROOM" : won ? "PLAY AGAIN" : "CONTINUE", true)) game.Advance();
        if (Button(cx - buttonWidth / 2, panel.y + 254, buttonWidth, "RESTART FROM LEVEL 1", false))
            game.RestartRun();
        if (Button(cx - buttonWidth / 2, panel.y + 312, buttonWidth, "LEVEL SELECTION", false))
            game.OpenLevelSelection();
    }

    void DrawVictory()
    {
        bool won = game.state == GameManager.State.Won;
        int level = LevelProgress.LevelForScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        bool newRecord = !hadPersonalBest || game.CompletionTime < previousPersonalBest;
        DarkHuntChapterTheme.Begin();
        try
        {
            DarkHuntChapterTheme.Title(new Rect(250, 96, 1100, 90), "YOU SURVIVED", 76);
            DarkHuntChapterTheme.Text(new Rect(450, 194, 700, 28), "N I G H T M A R E   C O N Q U E R E D", 16,
                color: DarkHuntChapterTheme.Ash);
            DarkHuntChapterTheme.Panel(new Rect(400, 250, 800, 470));
            DarkHuntChapterTheme.Emblem(new Vector2(800, 302));
            DarkHuntChapterTheme.Text(new Rect(450, 346, 700, 24), "CHAPTER COMPLETED  /  " + level.ToString("00"), 13,
                color: DarkHuntChapterTheme.Ash);
            DarkHuntChapterTheme.Title(new Rect(440, 380, 720, 42), DarkHuntChapterTheme.ChapterName(level), 32);
            DarkHuntChapterTheme.Fill(new Rect(456, 446, 688, 1), DarkHuntChapterTheme.Crimson);
            DarkHuntChapterTheme.Text(new Rect(456, 466, 320, 26), "COMPLETION TIME", 13, color: DarkHuntChapterTheme.Ash);
            DarkHuntChapterTheme.Text(new Rect(824, 466, 320, 26), "PERSONAL BEST", 13, color: DarkHuntChapterTheme.Ash);
            DarkHuntChapterTheme.Text(new Rect(456, 500, 320, 52),
                game.CompletionTime.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "s", 42, true);
            // Use the same saved best value as the HUD and chapter menu.
            string best = LevelProgress.TryGetBestTime(level, out float seconds)
                ? seconds.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "s" : "--";
            DarkHuntChapterTheme.Text(new Rect(824, 500, 320, 52), best, 42, true);
            DarkHuntChapterTheme.Fill(new Rect(800, 472, 1, 78), DarkHuntChapterTheme.Crimson);
            if (newRecord)
            {
                DarkHuntChapterTheme.Fill(new Rect(642, 578, 316, 32), DarkHuntChapterTheme.Crimson);
                DarkHuntChapterTheme.Text(new Rect(642, 578, 316, 32), "NEW PERSONAL RECORD", 13);
            }
            DarkHuntChapterTheme.Text(new Rect(456, 632, 688, 30),
                won ? $"Objects collected: {game.RunObjectsFound}" : $"Objects collected: {game.found} / {game.total}",
                16, color: DarkHuntChapterTheme.Ash);
            DarkHuntChapterTheme.Fill(new Rect(764, 690, 72, 1), DarkHuntChapterTheme.Blood);
            // Identical actions to the original successful-completion screen.
            if (DarkHuntChapterTheme.Button(new Rect(400, 756, 250, 52), won ? "PLAY AGAIN" : "NEXT NIGHTMARE", true))
                game.Advance();
            if (DarkHuntChapterTheme.Button(new Rect(675, 756, 250, 52), "RESTART FROM LEVEL 1"))
                game.RestartRun();
            if (DarkHuntChapterTheme.Button(new Rect(950, 756, 250, 52), "CHOOSE CHAPTER"))
                game.OpenLevelSelection();
        }
        finally { DarkHuntChapterTheme.End(); }
    }

}
