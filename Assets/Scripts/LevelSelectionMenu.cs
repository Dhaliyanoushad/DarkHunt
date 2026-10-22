using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class LevelSelectionMenu : MonoBehaviour
{
    static readonly Color Background = new Color(0.025f, 0.038f, 0.046f);
    static readonly Color Paper = new Color(0.91f, 0.94f, 0.92f);
    static readonly Color Muted = new Color(0.58f, 0.66f, 0.68f);
    static readonly Color Accent = new Color(0.66f, 0.77f, 0.73f);
    GUIStyle heading, caption, button, dialogHeading;
    bool loading;
    bool confirmingReset;

    void OnEnable()
    {
        Time.timeScale = 1f;
        confirmingReset = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnGUI()
    {
        if (heading == null)
        {
            heading = new GUIStyle(GUI.skin.label)
            { fontSize = 42, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            heading.normal.textColor = Paper;
            caption = new GUIStyle(heading)
            { fontSize = 16, fontStyle = FontStyle.Normal, wordWrap = true };
            caption.normal.textColor = Muted;
            button = new GUIStyle(caption) { fontSize = 20 };
            dialogHeading = new GUIStyle(heading) { fontSize = 24 };
        }
        Color previousColor = GUI.color;
        GUI.color = Background;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        if (confirmingReset)
        {
            DrawResetConfirmation();
            return;
        }

        float width = Mathf.Min(360, Screen.width - 40);
        float left = (Screen.width - width) / 2;
        float cy = Screen.height / 2;
        GUI.Label(new Rect(left - 12, cy - 224, width + 24, 56), "DARKHUNT", heading);
        GUI.Label(new Rect(left, cy - 166, width, 30), "LEVEL SELECTION", caption);
        DrawLevel(1, new Rect(left, cy - 116, width, 50));
        DrawLevel(2, new Rect(left, cy - 22, width, 50));
        GUI.Label(new Rect(left - 16, cy + 66, width + 32, 32),
            LevelProgress.IsUnlocked(2) ? "Choose a level to play." : "Complete Level 1 to unlock Level 2.", caption);
        if (DrawButton(new Rect(left, cy + 112, width, 46), "RESET PROGRESS", false, !loading))
            confirmingReset = true;
    }

    void DrawLevel(int level, Rect rect)
    {
        bool unlocked = LevelProgress.IsUnlocked(level);
        if (DrawButton(rect, "LEVEL " + level + (unlocked ? "  /  PLAY" : "  /  LOCKED"),
            true, unlocked && !loading)) PlayLevel(level);
        GUI.Label(new Rect(rect.x, rect.yMax + 4, rect.width, 26), LevelProgress.BestTimeLabel(level), caption);
    }

    bool DrawButton(Rect rect, string label, bool primary, bool enabled = true, bool danger = false)
    {
        bool hover = enabled && rect.Contains(Event.current.mousePosition);
        Color oldColor = GUI.color;
        Color fill = danger ? new Color(0.77f, 0.29f, 0.26f) : Accent;
        GUI.color = enabled && primary ? (hover ? Paper : fill) :
            (hover ? new Color(0.17f, 0.21f, 0.22f) : new Color(0.10f, 0.13f, 0.14f));
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = oldColor;
        button.normal.textColor = !enabled ? Muted : primary ? Background : Paper;
        button.hover.textColor = button.normal.textColor;
        button.active.textColor = button.normal.textColor;
        bool wasEnabled = GUI.enabled;
        GUI.enabled = wasEnabled && enabled;
        bool clicked = GUI.Button(rect, label, button);
        GUI.enabled = wasEnabled;
        return clicked;
    }

    void DrawResetConfirmation()
    {
        float width = Mathf.Min(420, Screen.width - 40);
        float left = (Screen.width - width) / 2;
        float top = Screen.height / 2 - 120;
        Color previousColor = GUI.color;
        GUI.color = new Color(0.055f, 0.075f, 0.08f);
        GUI.DrawTexture(new Rect(left, top, width, 240), Texture2D.whiteTexture);
        GUI.color = previousColor;
        GUI.Label(new Rect(left + 16, top + 18, width - 32, 40), "RESET PROGRESS?", dialogHeading);
        GUI.Label(new Rect(left + 20, top + 70, width - 40, 64),
            "Delete all best times and level unlocks?\nLevel 2 will be locked again.", caption);
        float buttonWidth = (width - 52) / 2;
        if (DrawButton(new Rect(left + 20, top + 164, buttonWidth, 48), "CANCEL", false))
            confirmingReset = false;
        if (DrawButton(new Rect(left + 32 + buttonWidth, top + 164, buttonWidth, 48),
            "RESET", true, danger: true))
        {
            LevelProgress.ResetSavedProgress();
            GameManager.ResetRunProgress();
            confirmingReset = false;
        }
    }

    public void PlayLevel(int level)
    {
        // Enforce the same rule even if invoked without the UI button.
        if (loading || confirmingReset || !LevelProgress.IsUnlocked(level)) return;
        loading = true;
        GameManager.ResetRunProgress();
        Time.timeScale = 1f;
        SceneManager.LoadScene(LevelProgress.SceneForLevel(level));
    }
}
