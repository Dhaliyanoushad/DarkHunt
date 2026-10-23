using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class LevelSelectionMenu : MonoBehaviour
{
    GUIStyle heading, caption, button, dialogHeading, dialogBody;
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
            heading = DarkHuntUITheme.Style(42, bold: true);
            caption = DarkHuntUITheme.Style(14, wrap: true);
            button = DarkHuntUITheme.Style(16);
            dialogHeading = DarkHuntUITheme.Style(23);
            dialogBody = DarkHuntUITheme.Style(15, wrap: true);
        }
        DarkHuntUITheme.Fill(new Rect(0, 0, Screen.width, Screen.height), DarkHuntUITheme.Background);

        if (confirmingReset)
        {
            DrawResetConfirmation();
            return;
        }

        float width = Mathf.Min(360, Screen.width - 40);
        float left = (Screen.width - width) / 2;
        float cy = Screen.height / 2;
        DarkHuntUITheme.Label(new Rect(left - 12, cy - 224, width + 24, 56), "DARKHUNT", heading, DarkHuntUITheme.Text);
        DarkHuntUITheme.Fill(new Rect(Screen.width / 2 - 20, cy - 162, 40, 1), DarkHuntUITheme.Highlight);
        DarkHuntUITheme.Label(new Rect(left, cy - 146, width, 24), "LEVEL SELECTION", caption, DarkHuntUITheme.Muted);
        for (int level = 1; level <= LevelProgress.LevelCount; level++)
            DrawLevel(level, new Rect(left, cy - 110 + (level - 1) * 76, width, 42));
        int nextLocked = LevelProgress.HighestUnlockedLevel + 1;
        DarkHuntUITheme.Label(new Rect(left - 16, cy + 118, width + 32, 24),
            nextLocked <= LevelProgress.LevelCount
                ? $"Complete Level {nextLocked - 1} to unlock Level {nextLocked}."
                : "Choose a level to play.", caption, DarkHuntUITheme.Muted);
        if (DrawButton(new Rect(left, cy + 160, width, 40), "RESET PROGRESS", false, !loading))
            confirmingReset = true;
        if (DrawButton(new Rect(left, cy + 212, width, 40), "BACK TO MAIN MENU", false,
            !loading && !confirmingReset)) BackToMainMenu();
    }

    void DrawLevel(int level, Rect rect)
    {
        bool unlocked = LevelProgress.IsUnlocked(level);
        if (DrawButton(rect, "LEVEL " + level + (unlocked ? "  /  PLAY" : "  /  LOCKED"),
            true, unlocked && !loading)) PlayLevel(level);
        DarkHuntUITheme.Label(new Rect(rect.x, rect.yMax + 6, rect.width, 20),
            LevelProgress.BestTimeLabel(level), caption, DarkHuntUITheme.Muted);
    }

    bool DrawButton(Rect rect, string label, bool primary, bool enabled = true, bool danger = false) =>
        DarkHuntUITheme.Button(rect, label, button, primary, enabled, danger);

    void DrawResetConfirmation()
    {
        float width = Mathf.Min(420, Screen.width - 40);
        float left = (Screen.width - width) / 2;
        float top = Screen.height / 2 - 126;
        DarkHuntUITheme.Panel(new Rect(left, top, width, 252));
        DarkHuntUITheme.Label(new Rect(left + 24, top + 24, width - 48, 34),
            "RESET PROGRESS?", dialogHeading, DarkHuntUITheme.Text);
        DarkHuntUITheme.Fill(new Rect(left + 24, top + 72, width - 48, 1), DarkHuntUITheme.Border);
        DarkHuntUITheme.Label(new Rect(left + 28, top + 88, width - 56, 60),
            "Delete all best times and level unlocks?\nLevels 2 and 3 will be locked again.", dialogBody, DarkHuntUITheme.Muted);
        float buttonWidth = (width - 60) / 2;
        if (DrawButton(new Rect(left + 24, top + 180, buttonWidth, 48), "CANCEL", false))
            confirmingReset = false;
        if (DrawButton(new Rect(left + 36 + buttonWidth, top + 180, buttonWidth, 48),
            "RESET", true, danger: true))
        {
            LevelProgress.ResetSavedProgress();
            GameManager.ResetRunProgress();
            confirmingReset = false;
        }
    }

    public void BackToMainMenu()
    {
        if (loading || confirmingReset) return;
        loading = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuDramatic");
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
