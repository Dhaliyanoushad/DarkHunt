using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class LevelSelectionMenu : MonoBehaviour
{
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
        DarkHuntChapterTheme.Begin();
        try
        {
            if (confirmingReset)
            {
                DrawResetConfirmation();
                return;
            }
            DarkHuntChapterTheme.Title(new Rect(300, 82, 1000, 76), "DARKHUNT", 64);
            DarkHuntChapterTheme.Text(new Rect(400, 164, 800, 28), "C H O O S E   Y O U R   N I G H T M A R E", 16, color: DarkHuntChapterTheme.Ash);
            DarkHuntChapterTheme.Fill(new Rect(768, 208, 64, 2), DarkHuntChapterTheme.Blood);
            for (int level = 1; level <= LevelProgress.LevelCount; level++)
                DrawLevel(level, new Rect(148 + (level - 1) * 444, 242, 416, 474));
            int nextLocked = LevelProgress.HighestUnlockedLevel + 1;
            DarkHuntChapterTheme.Text(new Rect(470, 772, 660, 48),
                nextLocked <= LevelProgress.LevelCount
                    ? $"Complete Chapter {nextLocked - 1:00} to unlock Chapter {nextLocked:00}."
                    : "Every door is open. Enter again if you dare.", 15, color: DarkHuntChapterTheme.Ash);
            if (DarkHuntChapterTheme.Button(new Rect(1152, 776, 300, 44), "RESET PROGRESS", enabled: !loading))
                confirmingReset = true;
            if (DarkHuntChapterTheme.Button(new Rect(148, 776, 300, 44), "BACK TO MAIN MENU",
                enabled: !loading && !confirmingReset)) BackToMainMenu();
        }
        finally { DarkHuntChapterTheme.End(); }
    }

    void DrawLevel(int level, Rect rect)
    {
        bool unlocked = LevelProgress.IsUnlocked(level);
        DarkHuntChapterTheme.Panel(rect);
        DarkHuntChapterTheme.Doorway(new Rect(rect.x, rect.y, rect.width, 260), unlocked);
        DarkHuntChapterTheme.Text(new Rect(rect.x + 30, rect.y + 30, rect.width - 60, 124),
            level.ToString("00"), 112, true, new Color(0.67f, 0.59f, 0.58f, unlocked ? 0.32f : 0.14f));
        if (!unlocked) DarkHuntChapterTheme.Lock(new Vector2(rect.center.x, rect.y + 199));
        DarkHuntChapterTheme.Text(new Rect(rect.x + 24, rect.y + 230, rect.width - 48, 24),
            unlocked ? "T H E   D O O R   A W A I T S" : "L O C K E D", 12,
            color: unlocked ? DarkHuntChapterTheme.Ash : DarkHuntChapterTheme.Bone);
        DarkHuntChapterTheme.Text(new Rect(rect.x + 24, rect.y + 278, rect.width - 48, 24),
            "CHAPTER " + level.ToString("00"), 13, color: DarkHuntChapterTheme.Ash);
        DarkHuntChapterTheme.Text(new Rect(rect.x + 24, rect.y + 310, rect.width - 48, 40),
            DarkHuntChapterTheme.ChapterName(level), 26, true);
        DarkHuntChapterTheme.Fill(new Rect(rect.center.x - 22, rect.y + 364, 44, 1), DarkHuntChapterTheme.Blood);
        DarkHuntChapterTheme.Text(new Rect(rect.x + 24, rect.y + 375, rect.width - 48, 28),
            LevelProgress.BestTimeLabel(level), 15, color: DarkHuntChapterTheme.Ash);
        if (DarkHuntChapterTheme.Button(new Rect(rect.x + 24, rect.y + 416, rect.width - 48, 42),
            "ENTER NIGHTMARE", true, unlocked && !loading)) PlayLevel(level);
    }

    void DrawResetConfirmation()
    {
        DarkHuntChapterTheme.Panel(new Rect(480, 296, 640, 308));
        DarkHuntChapterTheme.Title(new Rect(512, 322, 576, 48), "RESET PROGRESS?", 32);
        DarkHuntChapterTheme.Fill(new Rect(512, 388, 576, 1), DarkHuntChapterTheme.Crimson);
        DarkHuntChapterTheme.Text(new Rect(512, 408, 576, 32), "Delete all best times and level unlocks?", 18);
        DarkHuntChapterTheme.Text(new Rect(512, 444, 576, 28), "Levels 2 and 3 will be locked again.", 16, color: DarkHuntChapterTheme.Ash);
        if (DarkHuntChapterTheme.Button(new Rect(512, 520, 276, 48), "CANCEL"))
            confirmingReset = false;
        if (DarkHuntChapterTheme.Button(new Rect(812, 520, 276, 48), "RESET", true))
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
